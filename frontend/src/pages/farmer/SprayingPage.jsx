import { useEffect, useState } from "react";
import { getParcels } from "../../services/parcelService";
import {
  getAnnouncements,
  createAnnouncement,
  updateAnnouncement,
  cancelAnnouncement,
  getNotificationStatus,
} from "../../services/sprayingService";
import Modal from "../../components/Modal";
import SmartMap from "../../components/SmartMap";
import { toDatetimeLocalValue } from "../../utils/datetime";

const statusLabel = { Scheduled: "Zakazano", Completed: "Završeno", Cancelled: "Otkazano" };
const statusBadge = { Scheduled: "badge-yellow", Completed: "badge-green", Cancelled: "badge-red" };

export default function SprayingPage() {
  const [parcels, setParcels] = useState([]);
  const [announcements, setAnnouncements] = useState([]);
  const [form, setForm] = useState({ parcelId: "", plannedStartTime: "", durationHours: 1, substanceType: "" });
  const [error, setError] = useState("");
  const [editing, setEditing] = useState(null);
  const [editForm, setEditForm] = useState({ plannedStartTime: "", durationHours: 1, substanceType: "" });
  const [editError, setEditError] = useState("");
  const [notificationModal, setNotificationModal] = useState(null);
  const [notificationLoading, setNotificationLoading] = useState(false);

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
      const res = await createAnnouncement({ ...form, durationHours: parseInt(form.durationHours, 10) });
      alert(`Prskanje zakazano. Obavešteno pčelara: ${res.data.announcement.notifiedBeekeeperCount}`);
      setForm({ parcelId: "", plannedStartTime: "", durationHours: 1, substanceType: "" });
      if(res.data.weatherWarning){
        alert(res.data.weatherWarning);
      }
      load();
    } catch (err) {
      setError(err.response?.data?.message || "Greška pri zakazivanju prskanja.");
    }
  };

  const openEdit = (announcement) => {
    setEditing(announcement);
    setEditForm({
      plannedStartTime: toDatetimeLocalValue(announcement.plannedStartTime),
      durationHours: announcement.durationHours,
      substanceType: announcement.substanceType || "",
    });
    setEditError("");
  };

  const handleUpdate = async (e) => {
    e.preventDefault();
    setEditError("");
    try {
      await updateAnnouncement(editing.id, {
        plannedStartTime: editForm.plannedStartTime,
        durationHours: parseInt(editForm.durationHours, 10),
        substanceType: editForm.substanceType || null,
      });
      setEditing(null);
      alert("Termin je pomeren. Obližnji pčelari su obavešteni o izmeni.");
      load();
    } catch (err) {
      setEditError(err.response?.data?.message || "Greška pri izmeni termina.");
    }
  };

  const openNotificationStatus = async (announcement) => {
    setNotificationModal({ loading: true, announcement });
    setNotificationLoading(true);
    try {
      const res = await getNotificationStatus(announcement.id);
      setNotificationModal({ loading: false, announcement, status: res.data });
    } catch {
      setNotificationModal({
        loading: false,
        announcement,
        error: "Greška pri učitavanju statusa obaveštenja.",
      });
    } finally {
      setNotificationLoading(false);
    }
  };

  return (
    <div className="page">
      <div className="page-header">
        <h2>Najave prskanja</h2>
      </div>

      <div className="card" style={{ marginBottom: 24 }}>
        <h3>Zakaži prskanje</h3>
        <p className="card-meta">Kliknite na marker parcele na mapi ili izaberite iz liste.</p>
        {error && <div className="alert-error">{error}</div>}
        <div style={{ marginBottom: 16 }}>
          <SmartMap
            parcels={parcels}
            selectedParcelId={form.parcelId || null}
            onParcelSelect={(p) => setForm({ ...form, parcelId: p.id })}
            useCropIcons
          />
        </div>
        <form onSubmit={handleCreate}>
          <div className="form-row">
            <select value={form.parcelId} onChange={(e) => setForm({ ...form, parcelId: e.target.value })} required>
              <option value="">-- Odaberi parcelu --</option>
              {parcels.map((p) => (
                <option key={p.id} value={p.id}>{p.name}</option>
              ))}
            </select>
            <input
              type="datetime-local"
              value={form.plannedStartTime}
              onChange={(e) => setForm({ ...form, plannedStartTime: e.target.value })}
              required
            />
            <input
              type="number"
              min="1"
              placeholder="Trajanje (sati)"
              value={form.durationHours}
              onChange={(e) => setForm({ ...form, durationHours: e.target.value })}
              required
            />
            <input
              placeholder="Preparat (opciono)"
              value={form.substanceType}
              onChange={(e) => setForm({ ...form, substanceType: e.target.value })}
            />
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
                  <td>
                    <span className={`badge ${statusBadge[a.status] || "badge-yellow"}`}>
                      {statusLabel[a.status] || a.status}
                    </span>
                  </td>
                  <td>{a.notifiedBeekeeperCount}</td>
                  <td>
                    <div style={{ display: "flex", gap: 6, flexWrap: "wrap" }}>
                      <button
                        type="button"
                        className="btn-secondary btn-sm"
                        onClick={() => openNotificationStatus(a)}
                      >
                        Status
                      </button>
                      {a.status === "Scheduled" && (
                        <>
                          <button type="button" className="btn-secondary btn-sm" onClick={() => openEdit(a)}>
                            Pomeri
                          </button>
                          <button
                            type="button"
                            className="btn-danger btn-sm"
                            onClick={() => cancelAnnouncement(a.id).then(load)}
                          >
                            Otkaži
                          </button>
                        </>
                      )}
                    </div>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </div>

      {editing && (
        <Modal title={`Pomeri prskanje — ${editing.parcelName}`} onClose={() => setEditing(null)}>
          {editError && <div className="alert-error">{editError}</div>}
          <form onSubmit={handleUpdate}>
            <div className="form-group">
              <label>Novi datum i vreme početka</label>
              <input
                type="datetime-local"
                value={editForm.plannedStartTime}
                onChange={(e) => setEditForm({ ...editForm, plannedStartTime: e.target.value })}
                required
              />
            </div>
            <div className="form-group">
              <label>Trajanje (sati)</label>
              <input
                type="number"
                min="1"
                value={editForm.durationHours}
                onChange={(e) => setEditForm({ ...editForm, durationHours: e.target.value })}
                required
              />
            </div>
            <div className="form-group">
              <label>Preparat (opciono)</label>
              <input
                value={editForm.substanceType}
                onChange={(e) => setEditForm({ ...editForm, substanceType: e.target.value })}
                placeholder="Naziv preparata"
              />
            </div>
            <div style={{ display: "flex", gap: 8, justifyContent: "flex-end", marginTop: 16 }}>
              <button type="button" className="btn-secondary" onClick={() => setEditing(null)}>Otkaži</button>
              <button type="submit" className="btn-primary">Sačuvaj i obavesti pčelare</button>
            </div>
          </form>
        </Modal>
      )}

      {notificationModal && (
        <Modal
          title={`Status obaveštenja — ${notificationModal.announcement.parcelName}`}
          onClose={() => setNotificationModal(null)}
        >
          {notificationModal.loading || notificationLoading ? (
            <p>Učitavanje...</p>
          ) : notificationModal.error ? (
            <div className="alert-error">{notificationModal.error}</div>
          ) : (
            <dl className="detail-list">
              <dt>Parcela</dt>
              <dd>{notificationModal.announcement.parcelName}</dd>
              <dt>Planirani početak</dt>
              <dd>{new Date(notificationModal.status.plannedStartTime).toLocaleString("sr-RS")}</dd>
              <dt>Status najave</dt>
              <dd>{statusLabel[notificationModal.status.status] || notificationModal.status.status}</dd>
              <dt>Obavešteni pčelari (u radijusu 5 km)</dt>
              <dd><strong>{notificationModal.status.notifiedBeekeeperCount}</strong></dd>
            </dl>
          )}
        </Modal>
      )}
    </div>
  );
}
