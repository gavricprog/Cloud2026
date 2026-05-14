import { Navigate } from "react-router-dom";
import { isAuthenticated, getRole } from "../utils/auth";

export default function ProtectedRoute({ children, roles }) {
  if (!isAuthenticated()) return <Navigate to="/login" replace />;
  if (roles && !roles.includes(getRole())) return <Navigate to="/unauthorized" replace />;
  return children;
}
