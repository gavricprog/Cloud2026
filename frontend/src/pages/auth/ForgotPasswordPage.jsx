import { useState } from "react";
import { forgotPassword } from "../../services/authService";

export default function ForgotPasswordPage() {
  const [email, setEmail] = useState("");
  const [message, setMessage] = useState("");

  const handleSubmit = async (e) => {
    e.preventDefault();
    await forgotPassword(email);
    setMessage("Ukoliko nalog postoji, poslaćemo vam email.");
  };

  return (
    <div style={{ maxWidth: 400, margin: "80px auto", padding: 24 }}>
      <h2>Zaboravljena lozinka</h2>
      <form onSubmit={handleSubmit}>
        <input type="email" placeholder="Email adresa" value={email} onChange={(e) => setEmail(e.target.value)} required style={{ display: "block", width: "100%", marginBottom: 12 }} />
        <button type="submit">Pošalji link</button>
      </form>
      {message && <p>{message}</p>}
      <p><a href="/login">Nazad na prijavu</a></p>
    </div>
  );
}
