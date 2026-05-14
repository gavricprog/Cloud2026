import { useEffect, useState } from "react";
import { useParams, useNavigate } from "react-router-dom";
import { getApiaryHives } from "../../services/apiaryService";
import { createHive, deleteHive } from "../../services/hiveService";
import { HiveTypes } from "../../models";

export default function HivesPage() {
  const { apiaryId } = useParams();
  const [hives, setHives] = useState([]);
  const [form, setForm] = useState({ label: "", hiveType: "LR", frameColor: "", queenAge: 1, notes: "" });
  const navigate = useNavigate();

  useEffect(() => { load(); }, [apiaryId]);

  const load = async () => {
    const res = await getApiaryHives(apiaryId);
    setHives(res.data);
  };

  const handleCreate = async (e) => {
    e.preventDefault();
    await createHive(apiaryId, { ...form, queenAge: parseInt(form.queenAge) });
    setForm({ label: "", hiveType: "LR", frameColor: "", queenAge: 1, notes: "" });
    load();
  };

  return (
    <div className="page">
      <div className="page-header">
        <button className="btn-secondary" onClick={() => navigate("/apiaries")}>← Nazad</button>
        <h2 style={{ margin: 0 }}>Košnice pčelinjaka</h2>
        <div />
      </div>

      <div className="card" style={{ marginBottom: 24 }}>
        <h3>Dodaj košnicu</h3>
        <form onSubmit={handleCreate}>
          <div className="form-row">
            <input placeholder="Oznaka" value={form.label} onChange={(e) => setForm({ ...form, label: e.target.value })} required />
            <select value={form.hiveType} onChange={(e) => setForm({ ...form, hiveType: e.target.value })}>
              {HiveTypes.map((t) => <option key={t}>{t}</option>)}
            </select>
            <input placeholder="Boja nastavka" value={form.frameColor} onChange={(e) => setForm({ ...form, frameColor: e.target.value })} />
            <input placeholder="Starost matice (god)" type="number" min="0" value={form.queenAge} onChange={(e) => setForm({ ...form, queenAge: e.target.value })} />
            <input placeholder="Napomena" value={form.notes} onChange={(e) => setForm({ ...form, notes: e.target.value })} />
            <button type="submit" className="btn-primary">Dodaj košnicu</button>
          </div>
        </form>
      </div>

      <div className="card-grid">
        {hives.map((h) => (
          <div key={h.id} className="card">
            <div className="card-title">🍯 {h.label}</div>
            <div className="card-meta">
              Tip: {h.hiveType} · Boja: {h.frameColor || "—"}<br />
              Uređaj: <span className={`badge ${h.hasDevice ? "badge-green" : "badge-yellow"}`}>{h.hasDevice ? h.deviceStatus : "Nema"}</span>
            </div>
            <div className="card-actions">
              <button className="btn-secondary btn-sm" onClick={() => navigate(`/apiaries/${apiaryId}/hives/${h.id}/diary`)}>Dnevnik</button>
              <button className="btn-danger btn-sm" onClick={() => deleteHive(apiaryId, h.id).then(load)}>Obriši</button>
            </div>
          </div>
        ))}
      </div>

      {hives.length === 0 && (
        <div className="card" style={{ textAlign: "center", color: "var(--text-muted)", padding: 40 }}>
          Nema košnica u ovom pčelinjaku.
        </div>
      )}
    </div>
  );
}
