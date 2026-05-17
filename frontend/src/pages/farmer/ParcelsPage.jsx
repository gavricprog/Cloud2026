import { useCallback, useEffect, useState } from "react";
import { getParcels, createParcel, updateParcel, setCrop, deleteCrop } from "../../services/parcelService";
import { CropTypes } from "../../models";
import SmartMap from "../../components/SmartMap";

export default function ParcelsPage() {
  const [parcels, setParcels] = useState([]);
  const [form, setForm] = useState({ name: "", latitude: "", longitude: "", areaHectares: "" });
  const [editingId, setEditingId] = useState(null);
  const [editForm, setEditForm] = useState({ name: "", latitude: "", longitude: "", areaHectares: "" });
  const [cropForm, setCropForm] = useState({ parcelId: null, cropType: CropTypes[0], bloomingPeriod: "", additionalInfo: "" });

  const load = useCallback(async () => {
    const res = await getParcels();
    setParcels(res.data);
  }, []);

  useEffect(() => {
    let ignore = false;
    getParcels().then((res) => {
      if (!ignore) setParcels(res.data);
    });
    return () => { ignore = true; };
  }, []);

  const handleCreate = async (e) => {
    e.preventDefault();
    await createParcel({ ...form, latitude: parseFloat(form.latitude), longitude: parseFloat(form.longitude), areaHectares: form.areaHectares ? parseFloat(form.areaHectares) : null });
    setForm({ name: "", latitude: "", longitude: "", areaHectares: "" });
    load();
  };

  const startEdit = (p) => {
    setEditingId(p.id);
    setEditForm({
      name: p.name,
      latitude: p.latitude.toString(),
      longitude: p.longitude.toString(),
      areaHectares: p.areaHectares?.toString() ?? "",
    });
  };

  const handleUpdate = async (e) => {
    e.preventDefault();
    await updateParcel(editingId, {
      name: editForm.name,
      latitude: parseFloat(editForm.latitude),
      longitude: parseFloat(editForm.longitude),
      areaHectares: editForm.areaHectares ? parseFloat(editForm.areaHectares) : null,
    });
    setEditingId(null);
    load();
  };

  const handleSetCrop = async (e) => {
    e.preventDefault();
    await setCrop(cropForm.parcelId, {
      cropType: cropForm.cropType,
      bloomingPeriod: cropForm.bloomingPeriod,
      additionalInfo: cropForm.additionalInfo,
    });
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
        <p className="card-meta">Kliknite na mapu da izaberete tačnu lokaciju parcele.</p>
        <div style={{ marginBottom: 16 }}>
          <SmartMap
            parcels={parcels}
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
            <input placeholder="Naziv parcele" value={form.name} onChange={(e) => setForm({ ...form, name: e.target.value })} required />
            <input placeholder="Geografska širina" value={form.latitude} onChange={(e) => setForm({ ...form, latitude: e.target.value })} required />
            <input placeholder="Geografska dužina" value={form.longitude} onChange={(e) => setForm({ ...form, longitude: e.target.value })} required />
            <input type="number" placeholder="Površina (ha)" min="0" step="0.01" value={form.areaHectares} onChange={(e) => setForm({ ...form, areaHectares: e.target.value })} />
            <button type="submit" className="btn-primary">Dodaj parcelu</button>
          </div>
        </form>
      </div>

      <div className="card" style={{ marginBottom: 24 }}>
        <h3>Mapa parcela i posejanih kultura</h3>
        <p className="card-meta">Zeleni markeri prikazuju vaše parcele. Klik na marker prikazuje trenutno posejanu kulturu.</p>
        <SmartMap parcels={parcels} />
      </div>

      <div className="card-grid">
        {parcels.map((p) => (
          <div key={p.id} className="card">
            <div className="card-title">🌾 {p.name}</div>
            <div className="card-meta">
              📍 {p.latitude}, {p.longitude}
              {p.areaHectares && <><br />📐 {p.areaHectares} ha</>}
            </div>
            {p.currentCrop ? (
              <div style={{ marginBottom: 12 }}>
                <span className="badge badge-green">{p.currentCrop.cropType}</span>
                {p.currentCrop.bloomingPeriod && (
                  <span style={{ fontSize: 13, marginLeft: 8 }}>Cvetanje: {p.currentCrop.bloomingPeriod}</span>
                )}
              </div>
            ) : (
              <div style={{ marginBottom: 12, fontSize: 13, color: "var(--text-muted)" }}>Bez kulture</div>
            )}
            {editingId === p.id && (
              <form onSubmit={handleUpdate} style={{ marginTop: 10, borderTop: "1px solid #e5e7eb", paddingTop: 10 }}>
                <div style={{ display: "flex", flexDirection: "column", gap: 6 }}>
                  <input placeholder="Naziv" value={editForm.name} onChange={(e) => setEditForm({ ...editForm, name: e.target.value })} required />
                  <input placeholder="Geografska širina" value={editForm.latitude} onChange={(e) => setEditForm({ ...editForm, latitude: e.target.value })} required />
                  <input placeholder="Geografska dužina" value={editForm.longitude} onChange={(e) => setEditForm({ ...editForm, longitude: e.target.value })} required />
                  <input type="number" placeholder="Površina (ha, opciono)" min="0" step="0.01" value={editForm.areaHectares} onChange={(e) => setEditForm({ ...editForm, areaHectares: e.target.value })} />
                </div>
                <div style={{ display: "flex", gap: 8, marginTop: 8 }}>
                  <button type="submit" className="btn-primary btn-sm">Sačuvaj</button>
                  <button type="button" className="btn-secondary btn-sm" onClick={() => setEditingId(null)}>Otkaži</button>
                </div>
              </form>
            )}
            <div className="card-actions">
              <button type="button" className="btn-secondary btn-sm" onClick={() => startEdit(p)}>Izmeni lokaciju</button>
              {p.currentCrop ? (
                <button type="button" className="btn-secondary btn-sm" onClick={() => deleteCrop(p.id).then(load)}>Ukloni kulturu</button>
              ) : (
                <button type="button" className="btn-secondary btn-sm" onClick={() => setCropForm({ parcelId: p.id, cropType: CropTypes[0], bloomingPeriod: "", additionalInfo: "" })}>Dodaj kulturu</button>
              )}
            </div>

            {cropForm.parcelId === p.id && (
              <form onSubmit={handleSetCrop} style={{ marginTop: 12, borderTop: "1px solid #e5e7eb", paddingTop: 12 }}>
                <div style={{ display: "flex", flexDirection: "column", gap: 8 }}>
                  <select value={cropForm.cropType} onChange={(e) => setCropForm({ ...cropForm, cropType: e.target.value })}>
                    {CropTypes.map((c) => <option key={c}>{c}</option>)}
                  </select>
                  <input placeholder="Period cvetanja" value={cropForm.bloomingPeriod} onChange={(e) => setCropForm({ ...cropForm, bloomingPeriod: e.target.value })} required />
                  <input placeholder="Dodatne informacije" value={cropForm.additionalInfo} onChange={(e) => setCropForm({ ...cropForm, additionalInfo: e.target.value })} />
                  <div style={{ display: "flex", gap: 8 }}>
                    <button type="submit" className="btn-primary btn-sm">Sačuvaj</button>
                    <button type="button" className="btn-secondary btn-sm" onClick={() => setCropForm({ parcelId: null, cropType: CropTypes[0], bloomingPeriod: "", additionalInfo: "" })}>Otkaži</button>
                  </div>
                </div>
              </form>
            )}
          </div>
        ))}
      </div>
    </div>
  );
}
