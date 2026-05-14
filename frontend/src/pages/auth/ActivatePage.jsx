import { useState } from "react";
import { useSearchParams, useNavigate } from "react-router-dom";
import { activateAccount } from "../../services/authService";

export default function ActivatePage() {
  const [params] = useSearchParams();
  const [password, setPassword] = useState("");
  const [confirm, setConfirm] = useState("");
  const [message, setMessage] = useState("");
  const navigate = useNavigate();

  const handleSubmit = async (e) => {
    e.preventDefault();
    try {
      await activateAccount(params.get("token"), password, confirm);
      setMessage("Nalog aktiviran! Možete se prijaviti.");
      setTimeout(() => navigate("/login"), 2000);
    } catch (err) {
      setMessage(err.response?.data?.message ?? "Greška pri aktivaciji.");
    }
  };

  return (
    <div style={{ maxWidth: 400, margin: "80px auto", padding: 24 }}>
      <h2>Aktivacija naloga</h2>
      <form onSubmit={handleSubmit}>
        <input type="password" placeholder="Nova lozinka" value={password} onChange={(e) => setPassword(e.target.value)} required style={{ display: "block", width: "100%", marginBottom: 12 }} />
        <input type="password" placeholder="Potvrdi lozinku" value={confirm} onChange={(e) => setConfirm(e.target.value)} required style={{ display: "block", width: "100%", marginBottom: 12 }} />
        <button type="submit">Aktiviraj nalog</button>
      </form>
      {message && <p>{message}</p>}
    </div>
  );
}
