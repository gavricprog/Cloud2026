import { useEffect, useState } from "react";
import { useParams, useNavigate } from "react-router-dom";
import {
  BarChart,
  Bar,
  LineChart,
  Line,
  XAxis,
  YAxis,
  CartesianGrid,
  Tooltip,
  Legend,
  ResponsiveContainer,
  ReferenceLine,
} from "recharts";
import { getTelemetryCharts } from "../../services/telemetryService";

export default function TelemetryPage() {
  const { apiaryId } = useParams();
  const [data, setData] = useState(null);
  const [days, setDays] = useState(7);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");
  const navigate = useNavigate();

  useEffect(() => {
    load();
  }, [apiaryId, days]);

  const load = async () => {
    try {
      setLoading(true);
      setError("");

      const res = await getTelemetryCharts(apiaryId, days);
      setData(res.data);
    } catch (err) {
      console.error(err);
      setError("Greška pri učitavanju telemetrije.");
    } finally {
      setLoading(false);
    }
  };

  const dailyNectarData =
    data?.dailyNectar?.map((item) => ({
      date: new Date(item.date).toLocaleDateString("sr-RS"),
      delta: Number(item.delta.toFixed(2)),
    })) ?? [];

  const temperatureHumidityData =
    data?.temperatureHumidity?.map((item) => ({
      time: new Date(item.recordedAt).toLocaleString("sr-RS", {
        day: "2-digit",
        month: "2-digit",
        hour: "2-digit",
        minute: "2-digit",
      }),
      temperature: item.internalTemperature,
      humidity: item.humidity,
      weight: item.weight,
      batteryLevel: item.batteryLevel,
    })) ?? [];

  const latest = data?.latestReading;

  return (
    <div style={{ padding: 24 }}>
      <button onClick={() => navigate("/apiaries")}>← Nazad</button>

      <div style={{ display: "flex", justifyContent: "space-between", alignItems: "center", marginTop: 16 }}>
        <div>
          <h2>Telemetrija pčelinjaka</h2>
          <p style={{ color: "#666", marginTop: 4 }}>
            Pregled dnevnog prinosa, temperature, vlažnosti i poslednjeg stanja uređaja.
          </p>
        </div>

        <select value={days} onChange={(e) => setDays(Number(e.target.value))}>
          <option value={7}>7 dana</option>
          <option value={14}>14 dana</option>
          <option value={30}>30 dana</option>
        </select>
      </div>

      {loading && <p>Učitavanje telemetrije...</p>}
      {error && <div className="alert-error">{error}</div>}

      {!loading && !error && !data && (
        <p>Nema podataka za prikaz.</p>
      )}

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
                  <Bar
                    dataKey="delta"
                    name="Prinos / potrošnja"
                    fill="#4caf50"
                  />
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
                  <Line
                    type="monotone"
                    dataKey="temperature"
                    name="Temperatura (°C)"
                    stroke="#ff7300"
                    strokeWidth={2}
                    dot={false}
                  />
                  <Line
                    type="monotone"
                    dataKey="humidity"
                    name="Vlažnost (%)"
                    stroke="#387908"
                    strokeWidth={2}
                    dot={false}
                  />
                </LineChart>
              </ResponsiveContainer>
            )}
          </ChartCard>
        </>
      )}
    </div>
  );
}

function StatusCard({ title, value }) {
  return (
    <div
      style={{
        border: "1px solid #ddd",
        borderRadius: 12,
        padding: 16,
        minWidth: 180,
        background: "#fff",
        boxShadow: "0 2px 8px rgba(0,0,0,0.06)",
      }}
    >
      <div style={{ color: "#666", fontSize: 14 }}>{title}</div>
      <div style={{ fontSize: 24, fontWeight: 700, marginTop: 8 }}>{value}</div>
    </div>
  );
}

function ChartCard({ title, children }) {
  return (
    <div
      style={{
        border: "1px solid #ddd",
        borderRadius: 12,
        padding: 20,
        marginTop: 20,
        background: "#fff",
        boxShadow: "0 2px 8px rgba(0,0,0,0.06)",
      }}
    >
      <h3 style={{ marginTop: 0 }}>{title}</h3>
      {children}
    </div>
  );
}