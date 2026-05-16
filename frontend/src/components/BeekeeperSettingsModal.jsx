import { useEffect, useState } from "react";
import Modal from "./Modal";
import { getUserSettings, updateUserSettings } from "../services/profileService";

export default function BeekeeperSettingsModal({ onClose }) {
  const [threshold, setThreshold] = useState(10);
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState("");
  const [success, setSuccess] = useState("");

  useEffect(() => {
    getUserSettings()
      .then((res) => setThreshold(res.data.weightDropThreshold))
      .catch(() => setError("Greška pri učitavanju podešavanja."))
      .finally(() => setLoading(false));
  }, []);

  const handleSubmit = async (e) => {
    e.preventDefault();
    setError("");
    setSuccess("");
    setSaving(true);
    try {
      const res = await updateUserSettings({ weightDropThreshold: parseFloat(threshold) });
      setThreshold(res.data.weightDropThreshold);
      setSuccess("Podešavanja su sačuvana.");
    } catch (err) {
      setError(err.response?.data?.message || "Greška pri čuvanju.");
    } finally {
      setSaving(false);
    }
  };

  return (
    <Modal title="Podešavanja" onClose={onClose}>
      {loading ? (
        <p>Učitavanje...</p>
      ) : (
        <form onSubmit={handleSubmit}>
          <p className="card-meta" style={{ marginTop: 0 }}>
            Ako pad težine košnice premaši ovaj prag između dva merenja, dobijate hitno upozorenje (podrazumevano 10 kg).
          </p>
          <div className="form-group">
            <label htmlFor="weight-threshold">Prag pada težine (kg)</label>
            <input
              id="weight-threshold"
              type="number"
              min="0.1"
              max="100"
              step="0.1"
              value={threshold}
              onChange={(e) => setThreshold(e.target.value)}
              required
            />
          </div>
          {error && <p className="alert-error">{error}</p>}
          {success && <p className="alert-success">{success}</p>}
          <div style={{ display: "flex", gap: 8, justifyContent: "flex-end", marginTop: 16 }}>
            <button type="button" className="btn-secondary" onClick={onClose}>
              Zatvori
            </button>
            <button type="submit" className="btn-primary" disabled={saving}>
              {saving ? "Čuvanje..." : "Sačuvaj"}
            </button>
          </div>
        </form>
      )}
    </Modal>
  );
}
