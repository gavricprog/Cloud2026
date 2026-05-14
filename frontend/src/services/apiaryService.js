import api from "./api";

export const getApiaries = () => api.get("/api/apiaries");
export const getApiary = (id) => api.get(`/api/apiaries/${id}`);
export const createApiary = (data) => api.post("/api/apiaries", data);
export const updateApiary = (id, data) => api.put(`/api/apiaries/${id}`, data);
export const deleteApiary = (id) => api.delete(`/api/apiaries/${id}`);
export const getApiaryHives = (id) => api.get(`/api/apiaries/${id}/hives`);
