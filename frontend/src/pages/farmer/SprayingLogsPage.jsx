import { useEffect, useState } from "react";
import { getParcels } from "../../services/parcelService";
import { getSprayingLogs, exportSprayingLogsPdf } from "../../services/sprayingService";

export default function SprayingLogsPage() {
  const [parcels, setParcels] = useState([]);
  const [logs, setLogs] = useState([]);
  const [filters, setFilters] = useState({ parcelId: "", from: "", to: "" });
  const [exporting, setExporting] = useState(false);

  useEffect(() => {
    getParcels().then((r) => setParcels(r.data));
    load();
  }, []);

  const load = async () => {
    const res = await getSprayingLogs(filters.parcelId || null, filters.from || null, filters.to || null);
    setLogs(res.data);
  };

  const handleExportPdf = async () => {
    setExporting(true);
    try {
      const res = await exportSprayingLogsPdf(
        filters.parcelId || null,
        filters.from || null,
        filters.to || null
      );
      const url = window.URL.createObjectURL(new Blob([res.data], { type: "application/pdf" }));
      const link = document.createElement("a");
      link.href = url;
      link.download = `karton-prskanja-${new Date().toISOString().slice(0, 10)}.pdf`;
      link.click();
      window.URL.revokeObjectURL(url);
    } catch {
      alert("Greška pri izvozu PDF-a.");
    } finally {
      setExporting(false);
    }
  };

  return (
    <div className="page">
      <div className="page-header">
        <h2>Karton prskanja</h2>
      </div>

      <div className="card" style={{ marginBottom: 24 }}>
        <div className="form-row">
          <select value={filters.parcelId} onChange={(e) => setFilters({ ...filters, parcelId: e.target.value })}>
            <option value="">Sve parcele</option>
            {parcels.map((p) => <option key={p.id} value={p.id}>{p.name}</option>)}
          </select>
          <input type="date" value={filters.from} onChange={(e) => setFilters({ ...filters, from: e.target.value })} />
          <input type="date" value={filters.to} onChange={(e) => setFilters({ ...filters, to: e.target.value })} />
          <button type="button" className="btn-primary" onClick={load}>Filtriraj</button>
          <button type="button" className="btn-secondary" onClick={handleExportPdf} disabled={exporting}>
            {exporting ? "Izvoz..." : "Izvezi PDF"}
          </button>
        </div>
        <p className="card-meta" style={{ marginTop: 12, marginBottom: 0 }}>
          Završeni tretmani se automatski evidentiraju nakon isteka zakazanog trajanja.
        </p>
      </div>

      <div className="card">
        <div className="table-wrap">
          <table>
            <thead>
              <tr>
                <th>Parcela</th>
                <th>Kultura</th>
                <th>Preparat</th>
                <th>Početak</th>
                <th>Kraj</th>
                <th>Vetar (m/s)</th>
                <th>Padavine</th>
              </tr>
            </thead>
            <tbody>
              {logs.map((l) => (
                <tr key={l.id}>
                  <td>{l.parcelName}</td>
                  <td>{l.cropType ?? "—"}</td>
                  <td>{l.substanceUsed ?? "—"}</td>
                  <td>{new Date(l.actualStartTime).toLocaleString("sr-RS")}</td>
                  <td>{new Date(l.actualEndTime).toLocaleString("sr-RS")}</td>
                  <td>{l.windSpeed}</td>
                  <td>{l.precipitation}</td>
                </tr>
              ))}
              {logs.length === 0 && (
                <tr>
                  <td colSpan="7" style={{ textAlign: "center", color: "var(--text-muted)" }}>
                    Nema zapisa za odabrane filtere.
                  </td>
                </tr>
              )}
            </tbody>
          </table>
        </div>
      </div>
    </div>
  );
}
