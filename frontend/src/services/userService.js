import api from "./api";

export const getUsers = () => api.get("/api/users");
export const getUser = (id) => api.get(`/api/users/${id}`);
export const createUser = (data) => api.post("/api/users", data);
export const deleteUser = (id) => api.delete(`/api/users/${id}`);
export const suspendUser = (id) => api.patch(`/api/users/${id}/suspend`);
export const setUserPassword = (id, password) => api.patch(`/api/users/${id}/set-password`, { password });
