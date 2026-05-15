import { useCallback, useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import { getApiaries, createApiary, deleteApiary } from "../../services/apiaryService";
import { getPublicParcels } from "../../services/parcelService";
import SmartMap from "../../components/SmartMap";

export default function ApiariesPage() {
  const [apiaries, setApiaries] = useState([]);
  const [publicParcels, setPublicParcels] = useState([]);
  const [form, setForm] = useState({ name: "", latitude: "", longitude: "", description: "" });
  const [error, setError] = useState("");
  const navigate = useNavigate();

  const load = useCallback(async () => {
    const [apiariesRes, parcelsRes] = await Promise.all([getApiaries(), getPublicParcels()]);
    setApiaries(apiariesRes.data);
    setPublicParcels(parcelsRes.data);
  }, []);

  useEffect(() => {
    let ignore = false;

    Promise.all([getApiaries(), getPublicParcels()]).then(([apiariesRes, parcelsRes]) => {
      if (ignore) return;
      setApiaries(apiariesRes.data);
      setPublicParcels(parcelsRes.data);
    });

    return () => {
      ignore = true;
    };
  }, []);

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
            parcels={publicParcels}
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
        <p className="card-meta">Narandžasti markeri su vaši pčelinjaci, a zeleni markeri su parcele sa kulturama koje su uneli poljoprivrednici.</p>
        <SmartMap apiaries={apiaries} parcels={publicParcels} />
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
            <div className="card-actions">
              <button className="btn-secondary btn-sm" onClick={() => navigate(`/apiaries/${a.id}/hives`)}>Košnice</button>
              <button className="btn-secondary btn-sm" onClick={() => navigate(`/apiaries/${a.id}/telemetry`)}>Telemetrija</button>
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
