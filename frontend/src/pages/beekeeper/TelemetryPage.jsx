import { useEffect, useState } from "react";
import { useParams, useNavigate } from "react-router-dom";
import { getTelemetryCharts } from "../../services/telemetryService";

export default function TelemetryPage() {
  const { apiaryId } = useParams();
  const [data, setData] = useState(null);
  const [days, setDays] = useState(7);
  const navigate = useNavigate();

  useEffect(() => { load(); }, [days]);

  const load = async () => {
    const res = await getTelemetryCharts(apiaryId, days);
    setData(res.data);
  };

  return (
    <div style={{ padding: 24 }}>
      <button onClick={() => navigate("/apiaries")}>← Nazad</button>
      <h2>Telemetrija pčelinjaka</h2>
      <select value={days} onChange={(e) => setDays(parseInt(e.target.value))}>
        <option value={7}>7 dana</option>
        <option value={14}>14 dana</option>
        <option value={30}>30 dana</option>
      </select>

      {data?.latestReading && (
        <div style={{ display: "flex", gap: 16, margin: "16px 0" }}>
          <div style={{ border: "1px solid #ccc", padding: 12 }}>Težina: <strong>{data.latestReading.weight} kg</strong></div>
          <div style={{ border: "1px solid #ccc", padding: 12 }}>Temp: <strong>{data.latestReading.internalTemperature}°C</strong></div>
          <div style={{ border: "1px solid #ccc", padding: 12 }}>Vlaga: <strong>{data.latestReading.humidity}%</strong></div>
          <div style={{ border: "1px solid #ccc", padding: 12 }}>Baterija: <strong>{data.latestReading.batteryLevel}%</strong></div>
        </div>
      )}

      {data && (
        <div>
          <h3>Dnevni prinos/potrošnja nektara</h3>
          <table border="1" cellPadding="8">
            <thead><tr><th>Datum</th><th>Delta (kg)</th></tr></thead>
            <tbody>
              {data.dailyNectar.map((d) => (
                <tr key={d.date} style={{ color: d.delta >= 0 ? "green" : "red" }}>
                  <td>{new Date(d.date).toLocaleDateString("sr-RS")}</td>
                  <td>{d.delta >= 0 ? "+" : ""}{d.delta.toFixed(2)}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </div>
  );
}
