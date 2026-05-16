import { useCallback, useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import { getApiaries, createApiary, updateApiary, deleteApiary, uploadApiaryImage } from "../../services/apiaryService";
import { getNearbyParcels } from "../../services/parcelService";
import SmartMap from "../../components/SmartMap";

export default function ApiariesPage() {
  const [apiaries, setApiaries] = useState([]);
  const [nearbyParcels, setNearbyParcels] = useState([]);
  const [form, setForm] = useState({ name: "", latitude: "", longitude: "", description: "" });
  const [editingId, setEditingId] = useState(null);
  const [editForm, setEditForm] = useState({ name: "", latitude: "", longitude: "", description: "" });
  const [error, setError] = useState("");
  const navigate = useNavigate();

  const load = useCallback(async () => {
    const [apiariesRes, parcelsRes] = await Promise.all([getApiaries(), getNearbyParcels()]);
    setApiaries(apiariesRes.data);
    setNearbyParcels(parcelsRes.data);
  }, []);

  useEffect(() => {
    let ignore = false;

    Promise.all([getApiaries(), getNearbyParcels()]).then(([apiariesRes, parcelsRes]) => {
      if (ignore) return;
      setApiaries(apiariesRes.data);
      setNearbyParcels(parcelsRes.data);
    });

    return () => {
      ignore = true;
    };
  }, []);

  const startEdit = (a) => {
    setEditingId(a.id);
    setEditForm({ name: a.name, latitude: a.latitude.toString(), longitude: a.longitude.toString(), description: a.description || "" });
  };

  const handleUpdate = async (e) => {
    e.preventDefault();
    await updateApiary(editingId, { ...editForm, latitude: parseFloat(editForm.latitude), longitude: parseFloat(editForm.longitude) });
    setEditingId(null);
    load();
  };

  const handleCreate = async (e) => {
    e.preventDefault();
    setError("");
    try {
      await createApiary({ ...form, latitude: parseFloat(form.latitude), longitude: parseFloat(form.longitude) });
      setForm({ name: "", latitude: "", longitude: "", description: "" });
      load();
    } catch (err) {
      setError(err.response?.data?.message || "Greška pri dodavanju pčelinjaka.");
    }
  };

  return (
    <div className="page">
      <div className="page-header">
        <h2>Moji pčelinjaci</h2>
      </div>

      <div className="card" style={{ marginBottom: 24 }}>
        <h3>Dodaj pčelinjak</h3>
        <p className="card-meta">Kliknite na mapu da automatski popunite koordinate pčelinjaka.</p>
        {error && <div className="alert-error">{error}</div>}
        <div style={{ marginBottom: 16 }}>
          <SmartMap
            apiaries={apiaries}
            parcels={nearbyParcels}
            selectedLocation={{
              latitude: parseFloat(form.latitude),
              longitude: parseFloat(form.longitude),
            }}
            onLocationSelect={({ latitude, longitude }) => setForm({
              ...form,
              latitude: latitude.toString(),
              longitude: longitude.toString(),
            })}
          />
        </div>
        <form onSubmit={handleCreate}>
          <div className="form-row">
            <input placeholder="Naziv" value={form.name} onChange={(e) => setForm({ ...form, name: e.target.value })} required />
            <input placeholder="Geografska širina" value={form.latitude} onChange={(e) => setForm({ ...form, latitude: e.target.value })} required />
            <input placeholder="Geografska dužina" value={form.longitude} onChange={(e) => setForm({ ...form, longitude: e.target.value })} required />
            <input placeholder="Opis (opciono)" value={form.description} onChange={(e) => setForm({ ...form, description: e.target.value })} />
            <button type="submit" className="btn-primary">Dodaj pčelinjak</button>
          </div>
        </form>
      </div>

      <div className="card" style={{ marginBottom: 24 }}>
        <h3>Mapa pčelinjaka i posejanih kultura</h3>
        <p className="card-meta">Markeri sa slikom su vaši pčelinjaci. Emoji markeri su parcele sa kulturama u radijusu od 5 km.</p>
        <SmartMap apiaries={apiaries} parcels={nearbyParcels} />
      </div>

      <div className="card-grid">
        {apiaries.map((a) => (
          <div key={a.id} className="card">
            <div className="card-title">🏡 {a.name}</div>
            <div className="card-meta">
              📍 {a.latitude}, {a.longitude}<br />
              🐝 Košnice: {a.hiveCount}
              {a.description && <><br />{a.description}</>}
            </div>
            {editingId === a.id && (
              <form onSubmit={handleUpdate} style={{ marginTop: 10 }}>
                <div className="form-row">
                  <input placeholder="Naziv" value={editForm.name} onChange={(e) => setEditForm({ ...editForm, name: e.target.value })} required />
                  <input placeholder="Geografska širina" value={editForm.latitude} onChange={(e) => setEditForm({ ...editForm, latitude: e.target.value })} required />
                  <input placeholder="Geografska dužina" value={editForm.longitude} onChange={(e) => setEditForm({ ...editForm, longitude: e.target.value })} required />
                  <input placeholder="Opis (opciono)" value={editForm.description} onChange={(e) => setEditForm({ ...editForm, description: e.target.value })} />
                </div>
                <div style={{ display: "flex", gap: 8, marginTop: 8 }}>
                  <button type="submit" className="btn-primary btn-sm">Sačuvaj</button>
                  <button type="button" className="btn-secondary btn-sm" onClick={() => setEditingId(null)}>Otkaži</button>
                </div>
              </form>
            )}
            {a.imageUrl && (
              <img
                src={a.imageUrl}
                alt={a.name}
                style={{ width: "100%", maxHeight: 140, objectFit: "cover", borderRadius: 8, marginBottom: 10 }}
              />
            )}
            <label className="btn-secondary btn-sm" style={{ display: "inline-block", marginBottom: 8, cursor: "pointer" }}>
              {a.imageUrl ? "Promeni sliku" : "Dodaj sliku"}
              <input
                type="file"
                accept="image/jpeg,image/png,image/webp"
                hidden
                onChange={async (e) => {
                  const file = e.target.files?.[0];
                  if (!file) return;
                  await uploadApiaryImage(a.id, file);
                  load();
                  e.target.value = "";
                }}
              />
            </label>
            <div className="card-actions">
              <button type="button" className="btn-secondary btn-sm" onClick={() => navigate(`/apiaries/${a.id}/hives`)}>Košnice</button>
              <button type="button" className="btn-secondary btn-sm" onClick={() => navigate(`/apiaries/${a.id}/telemetry`)}>Telemetrija</button>
              <button className="btn-secondary btn-sm" onClick={() => startEdit(a)}>Izmeni</button>
              <button className="btn-danger btn-sm" onClick={() => deleteApiary(a.id).then(load)}>Obriši</button>
            </div>
          </div>
        ))}
      </div>

      {apiaries.length === 0 && (
        <div className="card" style={{ textAlign: "center", color: "var(--text-muted)", padding: 40 }}>
          Nemate još uvek nijedan pčelinjak. Dodajte prvi iznad.
        </div>
      )}
    </div>
  );
}
