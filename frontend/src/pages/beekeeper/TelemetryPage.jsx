import { useEffect, useRef, useState } from "react";
import { useNavigate } from "react-router-dom";
import * as signalR from "@microsoft/signalr";
import {
  BarChart, Bar, Cell, LineChart, Line,
  XAxis, YAxis, CartesianGrid, Tooltip, Legend,
  ResponsiveContainer, ReferenceLine,
} from "recharts";
import { getApiaries } from "../../services/apiaryService";
import { getTelemetryCharts } from "../../services/telemetryService";

const HUB_URL = import.meta.env.VITE_API_URL
  ? import.meta.env.VITE_API_URL.replace(/\/api$/, "") + "/hubs/telemetry"
  : "https://localhost:7014/hubs/telemetry";

export default function TelemetryPage() {
  const [apiaries, setApiaries] = useState([]);
  const [activeId, setActiveId] = useState(null);
  const [data, setData] = useState(null);
  const [days, setDays] = useState(7);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");
  const [signalRStatus, setSignalRStatus] = useState("Povezivanje...");
  const [lastUpdatedAt, setLastUpdatedAt] = useState(null);
  const connectionRef = useRef(null);
  const activeIdRef = useRef(null);
  const refreshTimeoutRef = useRef(null);
  const navigate = useNavigate();

  // Učitaj pčelinjake
  useEffect(() => {
    getApiaries().then((res) => {
      setApiaries(res.data);
      if (res.data.length > 0) setActiveId(res.data[0].id);
    });
  }, []);

  // Učitaj telemetriju kad se promijeni aktivan tab ili broj dana
  useEffect(() => {
    if (!activeId) return;

    activeIdRef.current = activeId;
    loadTelemetry(activeId, days);
  }, [activeId, days]);

  const loadTelemetry = async (apiaryId = activeId, selectedDays = days, showLoading = true) => {
    if (!apiaryId) return;

    try {
      if (showLoading) {
        setLoading(true);
      }

    setError("");

    const res = await getTelemetryCharts(apiaryId, selectedDays);
    setData(res.data);
    setLastUpdatedAt(new Date());
  } catch (err) {
    console.error("Greška pri učitavanju telemetrije:", err);
    setError("Greška pri učitavanju telemetrije.");
  } finally {
    if (showLoading) {
      setLoading(false);
    }
  }
};

  // SignalR — konekcija i Join/Leave po tabu
  useEffect(() => {
    const connection = new signalR.HubConnectionBuilder()
      .withUrl(HUB_URL, { withCredentials: false })
      .withAutomaticReconnect()
      .build();

    connection.on("TelemetryUpdate", () => {
      if (refreshTimeoutRef.current) {
        clearTimeout(refreshTimeoutRef.current);
      }

      refreshTimeoutRef.current = setTimeout(() => {
        const currentApiaryId = activeIdRef.current;

        if (currentApiaryId) {
          loadTelemetry(currentApiaryId, days, false);
        }
      }, 300);
    });

    connection.start().then(() => {
      connectionRef.current = connection;
      setSignalRStatus("Real-time povezan");

      if (activeIdRef.current) {
        connection.invoke("JoinApiary", activeIdRef.current);
      }
    }).catch(() => {
      setSignalRStatus("Real-time nije dostupan");
    });

    connection.onreconnecting(() => setSignalRStatus("Ponovno povezivanje..."));
    connection.onreconnected(() => setSignalRStatus("Real-time povezan"));
    connection.onclose(() => setSignalRStatus("Real-time prekinut"));

    return () => {
      if (refreshTimeoutRef.current) {
        clearTimeout(refreshTimeoutRef.current);
      }

      if (connectionRef.current) {
        if (activeIdRef.current) {
          connectionRef.current.invoke("LeaveApiary", activeIdRef.current).catch(() => {});
        }
      connectionRef.current.stop();
      connectionRef.current = null;
    }
    };
  }, []);

  // Join/Leave pri promjeni taba
  useEffect(() => {
    const conn = connectionRef.current;
    if (!conn || conn.state !== signalR.HubConnectionState.Connected) {
      activeIdRef.current = activeId;
      return;
    }
    const prev = activeIdRef.current;
    if (prev && prev !== activeId) {
      conn.invoke("LeaveApiary", prev).catch(() => {});
    }
    if (activeId) {
      conn.invoke("JoinApiary", activeId).catch(() => {});
    }
    activeIdRef.current = activeId;
  }, [activeId]);

  const switchTab = (id) => {
    setActiveId(id);
    setData(null);
  };

  const dailyNectarData =
    data?.dailyNectar?.map((item) => ({
      date: new Date(item.date).toLocaleDateString("sr-RS"),
      delta: Number(item.delta.toFixed(2)),
    })) ?? [];

  const temperatureHumidityData =
    data?.temperatureHumidity?.map((item) => ({
      time: new Date(item.recordedAt).toLocaleString("sr-RS", {
        day: "2-digit", month: "2-digit", hour: "2-digit", minute: "2-digit",
      }),
      temperature: item.internalTemperature,
      humidity: item.humidity,
      weight: item.weight,
      batteryLevel: item.batteryLevel,
    })) ?? [];

  const latest = data?.latestReading;

  return (
    <div style={{ padding: 24 }}>
      <div style={{ display: "flex", justifyContent: "space-between", alignItems: "center", marginBottom: 16 }}>
        <h2 style={{ margin: 0 }}>Telemetrija pčelinjaka</h2>
        <button onClick={() => navigate("/apiaries")} className="btn-secondary btn-sm">← Nazad</button>
      </div>

      {/* Tabovi */}
      {apiaries.length > 1 && (
        <div style={{ display: "flex", gap: 8, marginBottom: 20, borderBottom: "2px solid #eee", paddingBottom: 0 }}>
          {apiaries.map((a) => (
            <button
              key={a.id}
              type="button"
              onClick={() => switchTab(a.id)}
              style={{
                padding: "8px 18px",
                border: "none",
                borderBottom: activeId === a.id ? "3px solid #f59e0b" : "3px solid transparent",
                background: "none",
                fontWeight: activeId === a.id ? 700 : 400,
                cursor: "pointer",
                fontSize: 14,
                color: activeId === a.id ? "#f59e0b" : "#555",
              }}
            >
              🏡 {a.name}
            </button>
          ))}
        </div>
      )}

      <div style={{ display: "flex", justifyContent: "space-between", alignItems: "center", marginBottom: 16 }}>
        <div style={{ color: "#666", fontSize: 14 }}>
          <span>{signalRStatus}</span>
          {lastUpdatedAt && (
            <span>
              {" "}· Poslednje osvežavanje:{" "}
              {lastUpdatedAt.toLocaleTimeString("sr-RS", {
                hour: "2-digit",
                minute: "2-digit",
                second: "2-digit",
              })}
            </span>
          )}
        </div>

        <select value={days} onChange={(e) => setDays(Number(e.target.value))}>
          <option value={7}>7 dana</option>
          <option value={14}>14 dana</option>
          <option value={30}>30 dana</option>
        </select>
      </div>

      {loading && <p>Učitavanje telemetrije...</p>}
      {error && <div className="alert-error">{error}</div>}

      {!loading && !error && data && (
        <>
          <div style={{ display: "flex", gap: 16, margin: "20px 0", flexWrap: "wrap" }}>
            <StatusCard title="Trenutna težina" value={latest ? `${latest.weight} kg` : "N/A"} />
            <StatusCard title="Temperatura" value={latest ? `${latest.internalTemperature} °C` : "N/A"} />
            <StatusCard title="Vlažnost" value={latest ? `${latest.humidity} %` : "N/A"} />
            <StatusCard title="Baterija" value={latest ? `${latest.batteryLevel} %` : "N/A"} />
          </div>

          <ChartCard title="Dnevni prinos / potrošnja nektara">
            {dailyNectarData.length === 0 ? (
              <p>Nema dovoljno podataka za stubičasti grafikon.</p>
            ) : (
              <ResponsiveContainer width="100%" height={320}>
                <BarChart data={dailyNectarData}>
                  <CartesianGrid strokeDasharray="3 3" />
                  <XAxis dataKey="date" />
                  <YAxis unit=" kg" />
                  <Tooltip formatter={(value) => [`${value} kg`, "Delta"]} />
                  <Legend />
                  <ReferenceLine y={0} stroke="#000" />
                  <Bar dataKey="delta" name="Prinos / potrošnja">
                    {dailyNectarData.map((entry, index) => (
                      <Cell key={`cell-${index}`} fill={entry.delta >= 0 ? "#4caf50" : "#f44336"} />
                    ))}
                  </Bar>
                </BarChart>
              </ResponsiveContainer>
            )}
          </ChartCard>

          <ChartCard title="Trend temperature i vlažnosti">
            {temperatureHumidityData.length === 0 ? (
              <p>Nema dovoljno podataka za linijski grafikon.</p>
            ) : (
              <ResponsiveContainer width="100%" height={320}>
                <LineChart data={temperatureHumidityData}>
                  <CartesianGrid strokeDasharray="3 3" />
                  <XAxis dataKey="time" />
                  <YAxis />
                  <Tooltip />
                  <Legend />
                  <Line type="monotone" dataKey="temperature" name="Temperatura (°C)" stroke="#ff7300" strokeWidth={2} dot={false} />
                  <Line type="monotone" dataKey="humidity" name="Vlažnost (%)" stroke="#387908" strokeWidth={2} dot={false} />
                </LineChart>
              </ResponsiveContainer>
            )}
          </ChartCard>
        </>
      )}

      {!loading && !error && !data && <p>Nema podataka za prikaz.</p>}
    </div>
  );
}

function StatusCard({ title, value }) {
  return (
    <div style={{
      border: "1px solid #ddd", borderRadius: 12, padding: 16,
      minWidth: 180, background: "#fff", boxShadow: "0 2px 8px rgba(0,0,0,0.06)",
    }}>
      <div style={{ color: "#666", fontSize: 14 }}>{title}</div>
      <div style={{ fontSize: 24, fontWeight: 700, marginTop: 8 }}>{value}</div>
    </div>
  );
}

function ChartCard({ title, children }) {
  return (
    <div style={{
      border: "1px solid #ddd", borderRadius: 12, padding: 20,
      marginTop: 20, background: "#fff", boxShadow: "0 2px 8px rgba(0,0,0,0.06)",
    }}>
      <h3 style={{ marginTop: 0 }}>{title}</h3>
      {children}
    </div>
  );
}
