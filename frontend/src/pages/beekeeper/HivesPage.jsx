import { useEffect, useState } from "react";
import { useParams, useNavigate } from "react-router-dom";
import { getApiaryHives } from "../../services/apiaryService";
import { createHive, updateHive, deleteHive } from "../../services/hiveService";
import { registerDevice } from "../../services/deviceService";
import { HiveTypes } from "../../models";

export default function HivesPage() {
  const { apiaryId } = useParams();
  const [hives, setHives] = useState([]);
  const [form, setForm] = useState({ label: "", hiveType: "LR", frameColor: "", queenAge: 1, notes: "" });
  const [editingHiveId, setEditingHiveId] = useState(null);
  const [editForm, setEditForm] = useState({ label: "", hiveType: "LR", frameColor: "", queenAge: 1, notes: "" });
  const [registeringHiveId, setRegisteringHiveId] = useState(null);
  const [serialInput, setSerialInput] = useState("");
  const [registerError, setRegisterError] = useState("");
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

  const startEditHive = (h) => {
    setEditingHiveId(h.id);
    setEditForm({ label: h.label, hiveType: h.hiveType, frameColor: h.frameColor || "", queenAge: h.queenAge || 1, notes: h.notes || "" });
  };

  const handleUpdateHive = async (e) => {
    e.preventDefault();
    await updateHive(apiaryId, editingHiveId, { ...editForm, queenAge: parseInt(editForm.queenAge) });
    setEditingHiveId(null);
    load();
  };

  const handleRegisterDevice = async (hiveId) => {
    setRegisterError("");
    if (!serialInput.trim()) {
      setRegisterError("Unesite serijski broj.");
      return;
    }
    try {
      await registerDevice(apiaryId, hiveId, serialInput.trim());
      setRegisteringHiveId(null);
      setSerialInput("");
      load();
    } catch (err) {
      const msg = err.response?.data?.message || "Greška pri registraciji uređaja.";
      setRegisterError(msg);
    }
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
            {editingHiveId === h.id && (
              <form onSubmit={handleUpdateHive} style={{ marginTop: 10 }}>
                <div className="form-row">
                  <input placeholder="Oznaka" value={editForm.label} onChange={(e) => setEditForm({ ...editForm, label: e.target.value })} required />
                  <select value={editForm.hiveType} onChange={(e) => setEditForm({ ...editForm, hiveType: e.target.value })}>
                    {HiveTypes.map((t) => <option key={t}>{t}</option>)}
                  </select>
                  <input placeholder="Boja nastavka" value={editForm.frameColor} onChange={(e) => setEditForm({ ...editForm, frameColor: e.target.value })} />
                  <input type="number" min="0" placeholder="Starost matice" value={editForm.queenAge} onChange={(e) => setEditForm({ ...editForm, queenAge: e.target.value })} />
                  <input placeholder="Napomena" value={editForm.notes} onChange={(e) => setEditForm({ ...editForm, notes: e.target.value })} />
                </div>
                <div style={{ display: "flex", gap: 8, marginTop: 8 }}>
                  <button type="submit" className="btn-primary btn-sm">Sačuvaj</button>
                  <button type="button" className="btn-secondary btn-sm" onClick={() => setEditingHiveId(null)}>Otkaži</button>
                </div>
              </form>
            )}
            {registeringHiveId === h.id && (
              <div style={{ marginTop: 10 }}>
                <input
                  placeholder="SA-2024-12345"
                  value={serialInput}
                  onChange={(e) => setSerialInput(e.target.value)}
                  style={{ marginRight: 8, padding: "4px 8px", borderRadius: 6, border: "1px solid #ccc" }}
                />
                <button className="btn-primary btn-sm" onClick={() => handleRegisterDevice(h.id)}>Potvrdi</button>
                <button className="btn-secondary btn-sm" style={{ marginLeft: 4 }} onClick={() => { setRegisteringHiveId(null); setSerialInput(""); setRegisterError(""); }}>Otkaži</button>
                {registerError && <div style={{ color: "red", fontSize: 13, marginTop: 4 }}>{registerError}</div>}
              </div>
            )}
            <div className="card-actions">
              <button className="btn-secondary btn-sm" onClick={() => startEditHive(h)}>Izmeni</button>
              {!h.hasDevice && (
                <button className="btn-secondary btn-sm" onClick={() => { setRegisteringHiveId(h.id); setSerialInput(""); setRegisterError(""); }}>+ Registruj uređaj</button>
              )}
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
