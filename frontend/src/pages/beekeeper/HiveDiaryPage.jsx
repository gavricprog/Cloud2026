import { useEffect, useState } from "react";
import { useParams, useNavigate } from "react-router-dom";
import {
  getDiaryEntries,
  createDiaryEntry,
  updateDiaryEntry,
  deleteDiaryEntry,
} from "../../services/diaryService";
import Modal from "../../components/Modal";
import { toDatetimeLocalValue } from "../../utils/datetime";

const emptyForm = {
  inspectionDate: "",
  floorColor: "",
  honeyFrames: 0,
  honeyAmount: 0,
  broodFrames: 0,
  queenPresent: false,
  notes: "",
};

export default function HiveDiaryPage() {
  const { apiaryId, hiveId } = useParams();
  const [entries, setEntries] = useState([]);
  const [total, setTotal] = useState(0);
  const [page, setPage] = useState(1);
  const [form, setForm] = useState(emptyForm);
  const [editing, setEditing] = useState(null);
  const [editForm, setEditForm] = useState(emptyForm);
  const [editError, setEditError] = useState("");
  const navigate = useNavigate();

  useEffect(() => {
    load();
  }, [page, apiaryId, hiveId]);

  const load = async () => {
    const res = await getDiaryEntries(apiaryId, hiveId, page);
    setEntries(res.data.data);
    setTotal(res.data.total);
  };

  const parseForm = (data) => ({
    inspectionDate: data.inspectionDate,
    floorColor: data.floorColor,
    honeyFrames: parseInt(data.honeyFrames, 10),
    honeyAmount: parseFloat(data.honeyAmount),
    broodFrames: parseInt(data.broodFrames, 10),
    queenPresent: data.queenPresent,
    notes: data.notes,
  });

  const handleCreate = async (e) => {
    e.preventDefault();
    await createDiaryEntry(apiaryId, hiveId, parseForm(form));
    setForm(emptyForm);
    load();
  };

  const openEdit = (entry) => {
    setEditing(entry);
    setEditForm({
      inspectionDate: toDatetimeLocalValue(entry.inspectionDate),
      floorColor: entry.floorColor,
      honeyFrames: entry.honeyFrames,
      honeyAmount: entry.honeyAmount,
      broodFrames: entry.broodFrames,
      queenPresent: entry.queenPresent,
      notes: entry.notes || "",
    });
    setEditError("");
  };

  const handleUpdate = async (e) => {
    e.preventDefault();
    setEditError("");
    try {
      await updateDiaryEntry(apiaryId, hiveId, editing.id, parseForm(editForm));
      setEditing(null);
      load();
    } catch (err) {
      setEditError(err.response?.data?.message || "Greška pri izmeni zapisa.");
    }
  };

  const totalPages = Math.ceil(total / 10);

  return (
    <div className="page">
      <div className="page-header">
        <button type="button" className="btn-secondary" onClick={() => navigate(`/apiaries/${apiaryId}/hives`)}>
          ← Nazad
        </button>
        <h2 style={{ margin: 0 }}>Dnevnik košnice</h2>
        <div />
      </div>

      <div className="card" style={{ marginBottom: 24 }}>
        <h3>Novi zapis</h3>
        <form onSubmit={handleCreate}>
          <div className="form-row">
            <input
              type="datetime-local"
              value={form.inspectionDate}
              onChange={(e) => setForm({ ...form, inspectionDate: e.target.value })}
              required
            />
            <input
              placeholder="Boja podnjače"
              value={form.floorColor}
              onChange={(e) => setForm({ ...form, floorColor: e.target.value })}
              required
            />
            <input
              type="number"
              placeholder="Ramovi sa medom"
              min="0"
              value={form.honeyFrames}
              onChange={(e) => setForm({ ...form, honeyFrames: e.target.value })}
            />
            <input
              type="number"
              placeholder="Med (kg)"
              min="0"
              step="0.1"
              value={form.honeyAmount}
              onChange={(e) => setForm({ ...form, honeyAmount: e.target.value })}
            />
            <input
              type="number"
              placeholder="Ramovi sa leglom"
              min="0"
              value={form.broodFrames}
              onChange={(e) => setForm({ ...form, broodFrames: e.target.value })}
            />
            <input
              placeholder="Napomena"
              value={form.notes}
              onChange={(e) => setForm({ ...form, notes: e.target.value })}
            />
          </div>
          <div style={{ display: "flex", alignItems: "center", gap: 16 }}>
            <label style={{ display: "flex", alignItems: "center", gap: 6, fontSize: 14 }}>
              <input
                type="checkbox"
                checked={form.queenPresent}
                onChange={(e) => setForm({ ...form, queenPresent: e.target.checked })}
              />
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
              {entries.map((entry) => (
                <tr key={entry.id}>
                  <td>{new Date(entry.inspectionDate).toLocaleString("sr-RS")}</td>
                  <td>{entry.floorColor}</td>
                  <td>{entry.honeyFrames} ram. / {entry.honeyAmount} kg</td>
                  <td>{entry.broodFrames} ram.</td>
                  <td>
                    <span className={`badge ${entry.queenPresent ? "badge-green" : "badge-red"}`}>
                      {entry.queenPresent ? "Da" : "Ne"}
                    </span>
                  </td>
                  <td>{entry.notes || "—"}</td>
                  <td>
                    <div style={{ display: "flex", gap: 6 }}>
                      <button type="button" className="btn-secondary btn-sm" onClick={() => openEdit(entry)}>
                        Izmeni
                      </button>
                      <button
                        type="button"
                        className="btn-danger btn-sm"
                        onClick={() => deleteDiaryEntry(apiaryId, hiveId, entry.id).then(load)}
                      >
                        Obriši
                      </button>
                    </div>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
        {totalPages > 1 && (
          <div style={{ display: "flex", alignItems: "center", gap: 12, marginTop: 16, justifyContent: "center" }}>
            <button type="button" className="btn-secondary btn-sm" disabled={page === 1} onClick={() => setPage((p) => p - 1)}>
              ←
            </button>
            <span style={{ fontSize: 14 }}>Strana {page} / {totalPages}</span>
            <button
              type="button"
              className="btn-secondary btn-sm"
              disabled={page >= totalPages}
              onClick={() => setPage((p) => p + 1)}
            >
              →
            </button>
          </div>
        )}
      </div>

      {editing && (
        <Modal title="Izmeni zapis u dnevniku" onClose={() => setEditing(null)}>
          {editError && <p className="alert-error">{editError}</p>}
          <form onSubmit={handleUpdate}>
            <div className="form-group">
              <label>Datum i vreme pregleda</label>
              <input
                type="datetime-local"
                value={editForm.inspectionDate}
                onChange={(e) => setEditForm({ ...editForm, inspectionDate: e.target.value })}
                required
              />
            </div>
            <div className="form-group">
              <label>Boja podnjače</label>
              <input
                value={editForm.floorColor}
                onChange={(e) => setEditForm({ ...editForm, floorColor: e.target.value })}
                required
              />
            </div>
            <div className="form-group">
              <label>Ramovi sa medom</label>
              <input
                type="number"
                min="0"
                value={editForm.honeyFrames}
                onChange={(e) => setEditForm({ ...editForm, honeyFrames: e.target.value })}
              />
            </div>
            <div className="form-group">
              <label>Med (kg)</label>
              <input
                type="number"
                min="0"
                step="0.1"
                value={editForm.honeyAmount}
                onChange={(e) => setEditForm({ ...editForm, honeyAmount: e.target.value })}
              />
            </div>
            <div className="form-group">
              <label>Ramovi sa leglom</label>
              <input
                type="number"
                min="0"
                value={editForm.broodFrames}
                onChange={(e) => setEditForm({ ...editForm, broodFrames: e.target.value })}
              />
            </div>
            <div className="form-group">
              <label>Napomena</label>
              <input
                value={editForm.notes}
                onChange={(e) => setEditForm({ ...editForm, notes: e.target.value })}
              />
            </div>
            <label style={{ display: "flex", alignItems: "center", gap: 6, fontSize: 14, marginBottom: 16 }}>
              <input
                type="checkbox"
                checked={editForm.queenPresent}
                onChange={(e) => setEditForm({ ...editForm, queenPresent: e.target.checked })}
              />
              Matica prisutna
            </label>
            <div style={{ display: "flex", gap: 8, justifyContent: "flex-end" }}>
              <button type="button" className="btn-secondary" onClick={() => setEditing(null)}>Otkaži</button>
              <button type="submit" className="btn-primary">Sačuvaj</button>
            </div>
          </form>
        </Modal>
      )}
    </div>
  );
}
