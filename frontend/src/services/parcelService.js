import api from "./api";

export const getParcels = () => api.get("/api/parcels");
export const getParcel = (id) => api.get(`/api/parcels/${id}`);
export const createParcel = (data) => api.post("/api/parcels", data);
export const updateParcel = (id, data) => api.put(`/api/parcels/${id}`, data);
export const setCrop = (id, data) => api.put(`/api/parcels/${id}/crop`, data);
export const deleteCrop = (id) => api.delete(`/api/parcels/${id}/crop`);
export const getPublicParcels = () => api.get("/api/parcels/public");
export const getNearbyParcels = (apiaryId) => {
  const params = apiaryId ? `?apiaryId=${apiaryId}` : "";
  return api.get(`/api/parcels/nearby${params}`);
};
