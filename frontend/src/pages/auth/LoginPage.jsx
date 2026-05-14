import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { login } from "../../services/authService";
import { setAuth } from "../../utils/auth";

export default function LoginPage() {
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [error, setError] = useState("");
  const navigate = useNavigate();

  const handleSubmit = async (e) => {
    e.preventDefault();
    try {
      const res = await login(email, password);
      setAuth(res.data.token, { email: res.data.email, fullName: res.data.fullName, role: res.data.role });
      if (res.data.role === "Admin") navigate("/admin/users");
      else if (res.data.role === "Beekeeper") navigate("/apiaries");
      else navigate("/parcels");
    } catch {
      setError("Pogrešan email ili lozinka.");
    }
  };

  return (
    <div className="login-wrap">
      <div className="login-card">
        <div className="login-logo"><span>🐝</span></div>
        <div className="login-title">Smart Apiary</div>
        <div className="login-subtitle">Prijavite se na vaš nalog</div>
        <form onSubmit={handleSubmit} autoComplete="off">
          <div className="form-group">
            <label>Email</label>
            <input type="text" autoComplete="new-password" value={email} onChange={(e) => setEmail(e.target.value)} required placeholder="vas@email.com" />
          </div>
          <div className="form-group">
            <label>Lozinka</label>
            <input type="password" autoComplete="new-password" value={password} onChange={(e) => setPassword(e.target.value)} required placeholder="••••••••" />
          </div>
          {error && <div className="alert-error">{error}</div>}
          <button type="submit" className="btn-primary btn-block">Prijavi se</button>
        </form>
        <p style={{ textAlign: "center", marginTop: 16, fontSize: 14 }}>
          <a href="/forgot-password">Zaboravili ste lozinku?</a>
        </p>
      </div>
    </div>
  );
}
