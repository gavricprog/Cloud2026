import { useEffect, useState } from "react";
import { getUsers, createUser, deleteUser, suspendUser, setUserPassword } from "../../services/userService";

export default function AdminUsersPage() {
  const [users, setUsers] = useState([]);
  const [form, setForm] = useState({ firstName: "", lastName: "", email: "", phone: "", role: "Beekeeper" });
  const [activatingId, setActivatingId] = useState(null);
  const [activatingPw, setActivatingPw] = useState("");
  const [error, setError] = useState("");

  useEffect(() => { loadUsers(); }, []);

  const loadUsers = async () => {
    const res = await getUsers();
    setUsers(res.data);
  };

  const handleCreate = async (e) => {
    e.preventDefault();
    setError("");
    try {
      await createUser(form);
      setForm({ firstName: "", lastName: "", email: "", phone: "", role: "Beekeeper" });
      loadUsers();
    } catch (err) {
      setError(err.response?.data?.message || "Greška pri kreiranju korisnika.");
    }
  };

  return (
    <div className="page">
      <div className="page-header">
        <h2>Upravljanje korisnicima</h2>
      </div>

      <div className="card" style={{ marginBottom: 24 }}>
        <h3>Novi korisnik</h3>
        {error && <div className="alert-error">{error}</div>}
        <form onSubmit={handleCreate}>
          <div className="form-row">
            <input placeholder="Ime" value={form.firstName} onChange={(e) => setForm({ ...form, firstName: e.target.value })} required />
            <input placeholder="Prezime" value={form.lastName} onChange={(e) => setForm({ ...form, lastName: e.target.value })} required />
            <input placeholder="Email" type="email" value={form.email} onChange={(e) => setForm({ ...form, email: e.target.value })} required />
            <input placeholder="Telefon" value={form.phone} onChange={(e) => setForm({ ...form, phone: e.target.value })} />
            <select value={form.role} onChange={(e) => setForm({ ...form, role: e.target.value })}>
              <option value="Beekeeper">Pčelar</option>
              <option value="Farmer">Poljoprivrednik</option>
            </select>
            <button type="submit" className="btn-primary">Kreiraj korisnika</button>
          </div>
        </form>
      </div>

      <div className="card">
        <div className="table-wrap">
          <table>
            <thead>
              <tr>
                <th>Ime i prezime</th>
                <th>Email</th>
                <th>Uloga</th>
                <th>Status</th>
                <th>Akcije</th>
              </tr>
            </thead>
            <tbody>
              {users.map((u) => (
                <tr key={u.id}>
                  <td><strong>{u.firstName} {u.lastName}</strong></td>
                  <td>{u.email}</td>
                  <td>{u.role === "Beekeeper" ? "Pčelar" : u.role === "Farmer" ? "Poljoprivrednik" : u.role}</td>
                  <td>
                    <span className={`badge ${u.isActive ? "badge-green" : "badge-red"}`}>
                      {u.isActive ? "Aktivan" : "Neaktivan"}
                    </span>
                  </td>
                  <td>
                    <div style={{ display: "flex", gap: 6 }}>
                      {!u.isActive && activatingId !== u.id && (
                        <button className="btn-secondary btn-sm" onClick={() => { setActivatingId(u.id); setActivatingPw(""); }}>Aktiviraj</button>
                      )}
                      {activatingId === u.id && (
                        <span style={{ display: "flex", gap: 4, alignItems: "center" }}>
                          <input type="password" placeholder="Nova lozinka" value={activatingPw} onChange={(e) => setActivatingPw(e.target.value)} style={{ width: 130, padding: "3px 8px", fontSize: 13 }} />
                          <button className="btn-primary btn-sm" onClick={() => setUserPassword(u.id, activatingPw).then(() => { loadUsers(); setActivatingId(null); })}>OK</button>
                          <button className="btn-secondary btn-sm" onClick={() => setActivatingId(null)}>✕</button>
                        </span>
                      )}
                      <button className="btn-secondary btn-sm" onClick={() => suspendUser(u.id).then(loadUsers)}>Suspenduj</button>
                      <button className="btn-danger btn-sm" onClick={() => deleteUser(u.id).then(loadUsers)}>Obriši</button>
                    </div>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </div>
    </div>
  );
}
