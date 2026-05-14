import { useEffect, useState } from "react";
import { useParams, useNavigate } from "react-router-dom";
import { getDiaryEntries, createDiaryEntry, deleteDiaryEntry } from "../../services/diaryService";

export default function HiveDiaryPage() {
  const { apiaryId, hiveId } = useParams();
  const [entries, setEntries] = useState([]);
  const [total, setTotal] = useState(0);
  const [page, setPage] = useState(1);
  const [form, setForm] = useState({ inspectionDate: "", floorColor: "", honeyFrames: 0, honeyAmount: 0, broodFrames: 0, queenPresent: false, notes: "" });
  const navigate = useNavigate();

  useEffect(() => { load(); }, [page]);

  const load = async () => {
    const res = await getDiaryEntries(apiaryId, hiveId, page);
    setEntries(res.data.data);
    setTotal(res.data.total);
  };

  const handleCreate = async (e) => {
    e.preventDefault();
    await createDiaryEntry(apiaryId, hiveId, { ...form, honeyFrames: parseInt(form.honeyFrames), honeyAmount: parseFloat(form.honeyAmount), broodFrames: parseInt(form.broodFrames) });
    setForm({ inspectionDate: "", floorColor: "", honeyFrames: 0, honeyAmount: 0, broodFrames: 0, queenPresent: false, notes: "" });
    load();
  };

  const totalPages = Math.ceil(total / 10);

  return (
    <div className="page">
      <div className="page-header">
        <button className="btn-secondary" onClick={() => navigate(`/apiaries/${apiaryId}/hives`)}>← Nazad</button>
        <h2 style={{ margin: 0 }}>Dnevnik košnice</h2>
        <div />
      </div>

      <div className="card" style={{ marginBottom: 24 }}>
        <h3>Novi zapis</h3>
        <form onSubmit={handleCreate}>
          <div className="form-row">
            <input type="datetime-local" value={form.inspectionDate} onChange={(e) => setForm({ ...form, inspectionDate: e.target.value })} required />
            <input placeholder="Boja podnjače" value={form.floorColor} onChange={(e) => setForm({ ...form, floorColor: e.target.value })} required />
            <input type="number" placeholder="Ramovi sa medom" min="0" value={form.honeyFrames} onChange={(e) => setForm({ ...form, honeyFrames: e.target.value })} />
            <input type="number" placeholder="Med (kg)" min="0" step="0.1" value={form.honeyAmount} onChange={(e) => setForm({ ...form, honeyAmount: e.target.value })} />
            <input type="number" placeholder="Ramovi sa leglom" min="0" value={form.broodFrames} onChange={(e) => setForm({ ...form, broodFrames: e.target.value })} />
            <input placeholder="Napomena" value={form.notes} onChange={(e) => setForm({ ...form, notes: e.target.value })} />
          </div>
          <div style={{ display: "flex", alignItems: "center", gap: 16 }}>
            <label style={{ display: "flex", alignItems: "center", gap: 6, fontSize: 14 }}>
              <input type="checkbox" checked={form.queenPresent} onChange={(e) => setForm({ ...form, queenPresent: e.target.checked })} />
              Matica prisutna
            </label>
            <button type="submit" className="btn-primary">Dodaj zapis</button>
          </div>
        </form>
      </div>

      <div className="card">
        <div className="table-wrap">
          <table>
            <thead>
              <tr>
                <th>Datum pregleda</th>
                <th>Boja podnjače</th>
                <th>Med</th>
                <th>Leglo</th>
                <th>Matica</th>
                <th>Napomena</th>
                <th></th>
              </tr>
            </thead>
            <tbody>
              {entries.map((e) => (
                <tr key={e.id}>
                  <td>{new Date(e.inspectionDate).toLocaleString("sr-RS")}</td>
                  <td>{e.floorColor}</td>
                  <td>{e.honeyFrames} ram. / {e.honeyAmount} kg</td>
                  <td>{e.broodFrames} ram.</td>
                  <td><span className={`badge ${e.queenPresent ? "badge-green" : "badge-red"}`}>{e.queenPresent ? "Da" : "Ne"}</span></td>
                  <td>{e.notes || "—"}</td>
                  <td><button className="btn-danger btn-sm" onClick={() => deleteDiaryEntry(apiaryId, hiveId, e.id).then(load)}>Obriši</button></td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
        {totalPages > 1 && (
          <div style={{ display: "flex", alignItems: "center", gap: 12, marginTop: 16, justifyContent: "center" }}>
            <button className="btn-secondary btn-sm" disabled={page === 1} onClick={() => setPage(p => p - 1)}>←</button>
            <span style={{ fontSize: 14 }}>Strana {page} / {totalPages}</span>
            <button className="btn-secondary btn-sm" disabled={page >= totalPages} onClick={() => setPage(p => p + 1)}>→</button>
          </div>
        )}
      </div>
    </div>
  );
}
