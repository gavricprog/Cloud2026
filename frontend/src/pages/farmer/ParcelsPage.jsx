import { useEffect, useState } from "react";
import { getParcels, createParcel, setCrop, deleteCrop } from "../../services/parcelService";
import { CropTypes } from "../../models";

export default function ParcelsPage() {
  const [parcels, setParcels] = useState([]);
  const [form, setForm] = useState({ name: "", latitude: "", longitude: "" });
  const [cropForm, setCropForm] = useState({ parcelId: null, cropType: CropTypes[0], bloomingPeriod: "", additionalInfo: "" });

  useEffect(() => { load(); }, []);

  const load = async () => {
    const res = await getParcels();
    setParcels(res.data);
  };

  const handleCreate = async (e) => {
    e.preventDefault();
    await createParcel({ ...form, latitude: parseFloat(form.latitude), longitude: parseFloat(form.longitude) });
    setForm({ name: "", latitude: "", longitude: "" });
    load();
  };

  const handleSetCrop = async (e) => {
    e.preventDefault();
    await setCrop(cropForm.parcelId, { cropType: cropForm.cropType, bloomingPeriod: cropForm.bloomingPeriod, additionalInfo: cropForm.additionalInfo });
    setCropForm({ parcelId: null, cropType: CropTypes[0], bloomingPeriod: "", additionalInfo: "" });
    load();
  };

  return (
    <div className="page">
      <div className="page-header">
        <h2>Moje parcele</h2>
      </div>

      <div className="card" style={{ marginBottom: 24 }}>
        <h3>Dodaj parcelu</h3>
        <form onSubmit={handleCreate}>
          <div className="form-row">
            <input placeholder="Naziv parcele" value={form.name} onChange={(e) => setForm({ ...form, name: e.target.value })} required />
            <input placeholder="Geografska širina" value={form.latitude} onChange={(e) => setForm({ ...form, latitude: e.target.value })} required />
            <input placeholder="Geografska dužina" value={form.longitude} onChange={(e) => setForm({ ...form, longitude: e.target.value })} required />
            <button type="submit" className="btn-primary">Dodaj parcelu</button>
          </div>
        </form>
      </div>

      <div className="card-grid">
        {parcels.map((p) => (
          <div key={p.id} className="card">
            <div className="card-title">🌾 {p.name}</div>
            <div className="card-meta">📍 {p.latitude}, {p.longitude}</div>
            {p.currentCrop ? (
              <div style={{ marginBottom: 12 }}>
                <span className="badge badge-green">{p.currentCrop.cropType}</span>
                {p.currentCrop.bloomingPeriod && <span style={{ fontSize: 13, marginLeft: 8 }}>Cvetanje: {p.currentCrop.bloomingPeriod}</span>}
              </div>
            ) : (
              <div style={{ marginBottom: 12, fontSize: 13, color: "var(--text-muted)" }}>Bez kulture</div>
            )}
            <div className="card-actions">
              {p.currentCrop
                ? <button className="btn-secondary btn-sm" onClick={() => deleteCrop(p.id).then(load)}>Ukloni kulturu</button>
                : <button className="btn-secondary btn-sm" onClick={() => setCropForm({ ...cropForm, parcelId: p.id })}>Dodaj kulturu</button>
              }
            </div>
          </div>
        ))}
      </div>

      {cropForm.parcelId && (
        <div className="card" style={{ marginTop: 24 }}>
          <h3>Dodaj kulturu</h3>
          <form onSubmit={handleSetCrop}>
            <div className="form-row">
              <select value={cropForm.cropType} onChange={(e) => setCropForm({ ...cropForm, cropType: e.target.value })}>
                {CropTypes.map((c) => <option key={c}>{c}</option>)}
              </select>
              <input placeholder="Period cvetanja" value={cropForm.bloomingPeriod} onChange={(e) => setCropForm({ ...cropForm, bloomingPeriod: e.target.value })} required />
              <input placeholder="Dodatne informacije" value={cropForm.additionalInfo} onChange={(e) => setCropForm({ ...cropForm, additionalInfo: e.target.value })} />
              <button type="submit" className="btn-primary">Sačuvaj</button>
              <button type="button" className="btn-secondary" onClick={() => setCropForm({ parcelId: null, cropType: CropTypes[0], bloomingPeriod: "", additionalInfo: "" })}>Otkaži</button>
            </div>
          </form>
        </div>
      )}
    </div>
  );
}
