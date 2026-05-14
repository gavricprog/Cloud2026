import api from "./api";

export const createHive = (apiaryId, data) =>
  api.post(`/api/apiaries/${apiaryId}/hives`, data);
export const getHive = (apiaryId, hiveId) =>
  api.get(`/api/apiaries/${apiaryId}/hives/${hiveId}`);
export const updateHive = (apiaryId, hiveId, data) =>
  api.put(`/api/apiaries/${apiaryId}/hives/${hiveId}`, data);
export const deleteHive = (apiaryId, hiveId) =>
  api.delete(`/api/apiaries/${apiaryId}/hives/${hiveId}`);
