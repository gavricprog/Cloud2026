import { useEffect, useState } from "react";
import { getParcels } from "../../services/parcelService";
import { getAnnouncements, createAnnouncement, cancelAnnouncement } from "../../services/sprayingService";

const statusLabel = { Scheduled: "Zakazano", Completed: "Završeno", Cancelled: "Otkazano" };
const statusBadge = { Scheduled: "badge-yellow", Completed: "badge-green", Cancelled: "badge-red" };

export default function SprayingPage() {
  const [parcels, setParcels] = useState([]);
  const [announcements, setAnnouncements] = useState([]);
  const [form, setForm] = useState({ parcelId: "", plannedStartTime: "", durationHours: 1, substanceType: "" });
  const [error, setError] = useState("");

  useEffect(() => {
    getParcels().then((r) => setParcels(r.data));
    load();
  }, []);

  const load = async () => {
    const res = await getAnnouncements();
    setAnnouncements(res.data);
  };

  const handleCreate = async (e) => {
    e.preventDefault();
    setError("");
    try {
      const res = await createAnnouncement({ ...form, durationHours: parseInt(form.durationHours) });
      alert(`Prskanje zakazano. Obavešteno pčelara: ${res.data.notifiedBeekeeperCount}`);
      setForm({ parcelId: "", plannedStartTime: "", durationHours: 1, substanceType: "" });
      load();
    } catch (err) {
      setError(err.response?.data?.message || "Greška pri zakazivanju prskanja.");
    }
  };

  return (
    <div className="page">
      <div className="page-header">
        <h2>Najave prskanja</h2>
      </div>

      <div className="card" style={{ marginBottom: 24 }}>
        <h3>Zakaži prskanje</h3>
        {error && <div className="alert-error">{error}</div>}
        <form onSubmit={handleCreate}>
          <div className="form-row">
            <select value={form.parcelId} onChange={(e) => setForm({ ...form, parcelId: e.target.value })} required>
              <option value="">-- Odaberi parcelu --</option>
              {parcels.map((p) => <option key={p.id} value={p.id}>{p.name}</option>)}
            </select>
            <input type="datetime-local" value={form.plannedStartTime} onChange={(e) => setForm({ ...form, plannedStartTime: e.target.value })} required />
            <input type="number" min="1" placeholder="Trajanje (sati)" value={form.durationHours} onChange={(e) => setForm({ ...form, durationHours: e.target.value })} required />
            <input placeholder="Preparat (opciono)" value={form.substanceType} onChange={(e) => setForm({ ...form, substanceType: e.target.value })} />
            <button type="submit" className="btn-primary">Zakaži prskanje</button>
          </div>
        </form>
      </div>

      <div className="card">
        <div className="table-wrap">
          <table>
            <thead>
              <tr>
                <th>Parcela</th>
                <th>Planirani početak</th>
                <th>Trajanje</th>
                <th>Status</th>
                <th>Obavešteni pčelari</th>
                <th></th>
              </tr>
            </thead>
            <tbody>
              {announcements.map((a) => (
                <tr key={a.id}>
                  <td><strong>{a.parcelName}</strong></td>
                  <td>{new Date(a.plannedStartTime).toLocaleString("sr-RS")}</td>
                  <td>{a.durationHours}h</td>
                  <td><span className={`badge ${statusBadge[a.status] || "badge-yellow"}`}>{statusLabel[a.status] || a.status}</span></td>
                  <td>{a.notifiedBeekeeperCount}</td>
                  <td>
                    {a.status === "Scheduled" && (
                      <button className="btn-danger btn-sm" onClick={() => cancelAnnouncement(a.id).then(load)}>Otkaži</button>
                    )}
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </div>
    </div>
  );
}
