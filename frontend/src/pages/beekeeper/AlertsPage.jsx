import { useEffect, useState } from "react";
import { getAlerts, markRead, markAllRead } from "../../services/alertService";

const typeLabel = {
  Theft: "Krađa / prevrtanje",
  BatteryLow: "Baterija ispod 15%",
  Pesticide: "Upozorenje o pesticidima",
  SprayingCancelled: "Prskanje otkazano",
  SprayingRescheduled: "Prskanje pomereno",
};

const typeBadge = {
  Theft: "badge-red",
  BatteryLow: "badge-yellow",
  Pesticide: "badge-red",
  SprayingCancelled: "badge-green",
  SprayingRescheduled: "badge-yellow",
};

export default function AlertsPage() {
  const [alerts, setAlerts] = useState([]);
  const [unreadOnly, setUnreadOnly] = useState(false);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    load();
  }, [unreadOnly]);

  const load = async () => {
    setLoading(true);
    const res = await getAlerts(unreadOnly);
    setAlerts(res.data);
    setLoading(false);
  };

  const handleMarkRead = async (id) => {
    await markRead(id);
    load();
  };

  const handleMarkAllRead = async () => {
    await markAllRead();
    load();
  };

  const unreadCount = alerts.filter((a) => !a.isRead).length;

  return (
    <div className="page">
      <div className="page-header">
        <h2>Upozorenja i notifikacije</h2>
        <div style={{ display: "flex", gap: 12, alignItems: "center" }}>
          <label style={{ display: "flex", alignItems: "center", gap: 6, fontSize: 14 }}>
            <input
              type="checkbox"
              checked={unreadOnly}
              onChange={(e) => setUnreadOnly(e.target.checked)}
            />
            Samo nepročitana
          </label>
          {unreadCount > 0 && (
            <button className="btn-secondary btn-sm" onClick={handleMarkAllRead}>
              Označi sve kao pročitano
            </button>
          )}
        </div>
      </div>

      {loading && <p>Učitavanje...</p>}

      {!loading && alerts.length === 0 && (
        <div className="card" style={{ textAlign: "center", color: "var(--text-muted)", padding: 40 }}>
          Nema upozorenja.
        </div>
      )}

      <div style={{ display: "flex", flexDirection: "column", gap: 12 }}>
        {alerts.map((a) => (
          <div
            key={a.id}
            className="card"
            style={{
              borderLeft: `4px solid ${a.isRead ? "#ddd" : "var(--primary)"}`,
              opacity: a.isRead ? 0.7 : 1,
            }}
          >
            <div style={{ display: "flex", justifyContent: "space-between", alignItems: "flex-start" }}>
              <div>
                <span className={`badge ${typeBadge[a.type] || "badge-yellow"}`}>
                  {typeLabel[a.type] || a.type}
                </span>
                <p style={{ margin: "8px 0 4px", fontWeight: a.isRead ? 400 : 600 }}>
                  {a.message}
                </p>
                <span style={{ fontSize: 13, color: "var(--text-muted)" }}>
                  {new Date(a.createdAt).toLocaleString("sr-RS")}
                </span>
              </div>
              {!a.isRead && (
                <button className="btn-secondary btn-sm" onClick={() => handleMarkRead(a.id)}>
                  Pročitano
                </button>
              )}
            </div>
          </div>
        ))}
      </div>
    </div>
  );
}
