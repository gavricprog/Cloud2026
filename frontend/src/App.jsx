import { BrowserRouter, Routes, Route, Navigate, useNavigate } from "react-router-dom";
import ProtectedRoute from "./components/ProtectedRoute";
import { Roles } from "./models";
import { clearAuth, getUser } from "./utils/auth";

import LoginPage from "./pages/auth/LoginPage";
import ActivatePage from "./pages/auth/ActivatePage";
import ForgotPasswordPage from "./pages/auth/ForgotPasswordPage";
import ResetPasswordPage from "./pages/auth/ResetPasswordPage";

import AdminUsersPage from "./pages/admin/AdminUsersPage";

import ApiariesPage from "./pages/beekeeper/ApiariesPage";
import HivesPage from "./pages/beekeeper/HivesPage";
import HiveDiaryPage from "./pages/beekeeper/HiveDiaryPage";
import TelemetryPage from "./pages/beekeeper/TelemetryPage";
import AlertsPage from "./pages/beekeeper/AlertsPage";

import ParcelsPage from "./pages/farmer/ParcelsPage";
import SprayingPage from "./pages/farmer/SprayingPage";
import SprayingLogsPage from "./pages/farmer/SprayingLogsPage";

function NavBar() {
  const navigate = useNavigate();
  const user = getUser();
  if (!user) return null;
  const handleLogout = () => { clearAuth(); navigate("/login"); };
  return (
    <nav className="navbar">
      <span className="navbar-brand" style={{ cursor: "pointer" }} onClick={() => {
        if (user.role === Roles.Beekeeper) navigate("/apiaries");
        else if (user.role === Roles.Farmer) navigate("/parcels");
        else if (user.role === Roles.Admin) navigate("/admin/users");
      }}>🐝 Smart Apiary</span>
      <div style={{ display: "flex", alignItems: "center", gap: 16 }}>
        {user.role === Roles.Beekeeper && (
          <button className="btn-secondary btn-sm" onClick={() => navigate("/alerts")}>🔔 Upozorenja</button>
        )}
        <span className="navbar-user">{user.fullName} · {user.role}</span>
        <button onClick={handleLogout} className="btn-danger btn-sm">Odjavi se</button>
      </div>
    </nav>
  );
}

export default function App() {
  return (
    <BrowserRouter>
      <NavBar />
      <Routes>
        <Route path="/login" element={<LoginPage />} />
        <Route path="/activate" element={<ActivatePage />} />
        <Route path="/forgot-password" element={<ForgotPasswordPage />} />
        <Route path="/reset-password" element={<ResetPasswordPage />} />

        <Route path="/admin/users" element={
          <ProtectedRoute roles={[Roles.Admin]}><AdminUsersPage /></ProtectedRoute>
        } />

        <Route path="/apiaries" element={
          <ProtectedRoute roles={[Roles.Beekeeper]}><ApiariesPage /></ProtectedRoute>
        } />
        <Route path="/apiaries/:apiaryId/hives" element={
          <ProtectedRoute roles={[Roles.Beekeeper]}><HivesPage /></ProtectedRoute>
        } />
        <Route path="/apiaries/:apiaryId/hives/:hiveId/diary" element={
          <ProtectedRoute roles={[Roles.Beekeeper]}><HiveDiaryPage /></ProtectedRoute>
        } />
        <Route path="/apiaries/:apiaryId/telemetry" element={
          <ProtectedRoute roles={[Roles.Beekeeper]}><TelemetryPage /></ProtectedRoute>
        } />
        <Route path="/alerts" element={
          <ProtectedRoute roles={[Roles.Beekeeper]}><AlertsPage /></ProtectedRoute>
        } />

        <Route path="/parcels" element={
          <ProtectedRoute roles={[Roles.Farmer]}><ParcelsPage /></ProtectedRoute>
        } />
        <Route path="/spraying" element={
          <ProtectedRoute roles={[Roles.Farmer]}><SprayingPage /></ProtectedRoute>
        } />
        <Route path="/spraying/logs" element={
          <ProtectedRoute roles={[Roles.Farmer]}><SprayingLogsPage /></ProtectedRoute>
        } />

        <Route path="/" element={<Navigate to="/login" replace />} />
        <Route path="/unauthorized" element={<div className="page"><h2>Nemate pristup ovoj stranici.</h2></div>} />
      </Routes>
    </BrowserRouter>
  );
}
