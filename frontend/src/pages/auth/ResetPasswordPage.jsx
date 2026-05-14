import { useState } from "react";
import { useSearchParams, useNavigate } from "react-router-dom";
import { resetPassword } from "../../services/authService";

export default function ResetPasswordPage() {
  const [params] = useSearchParams();
  const [password, setPassword] = useState("");
  const [confirm, setConfirm] = useState("");
  const [message, setMessage] = useState("");
  const navigate = useNavigate();

  const handleSubmit = async (e) => {
    e.preventDefault();
    try {
      await resetPassword(params.get("token"), password, confirm);
      setMessage("Lozinka promenjena! Preusmeravanje...");
      setTimeout(() => navigate("/login"), 2000);
    } catch (err) {
      setMessage(err.response?.data?.message ?? "Greška.");
    }
  };

  return (
    <div style={{ maxWidth: 400, margin: "80px auto", padding: 24 }}>
      <h2>Nova lozinka</h2>
      <form onSubmit={handleSubmit}>
        <input type="password" placeholder="Nova lozinka" value={password} onChange={(e) => setPassword(e.target.value)} required style={{ display: "block", width: "100%", marginBottom: 12 }} />
        <input type="password" placeholder="Potvrdi lozinku" value={confirm} onChange={(e) => setConfirm(e.target.value)} required style={{ display: "block", width: "100%", marginBottom: 12 }} />
        <button type="submit">Sačuvaj lozinku</button>
      </form>
      {message && <p>{message}</p>}
    </div>
  );
}
