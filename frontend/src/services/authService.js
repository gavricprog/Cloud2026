import api from "./api";

export const login = (email, password) =>
  api.post("/api/auth/login", { email, password });

export const activateAccount = (token, password, confirmPassword) =>
  api.post("/api/auth/activate", { token, password, confirmPassword });

export const forgotPassword = (email) =>
  api.post("/api/auth/forgot-password", { email });

export const resetPassword = (token, password, confirmPassword) =>
  api.post("/api/auth/reset-password", { token, password, confirmPassword });
