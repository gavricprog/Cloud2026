import api from "./api";

export const getDiaryEntries = (apiaryId, hiveId, page = 1) =>
  api.get(`/api/apiaries/${apiaryId}/hives/${hiveId}/diary?page=${page}`);
export const createDiaryEntry = (apiaryId, hiveId, data) =>
  api.post(`/api/apiaries/${apiaryId}/hives/${hiveId}/diary`, data);
export const updateDiaryEntry = (apiaryId, hiveId, entryId, data) =>
  api.put(`/api/apiaries/${apiaryId}/hives/${hiveId}/diary/${entryId}`, data);
export const deleteDiaryEntry = (apiaryId, hiveId, entryId) =>
  api.delete(`/api/apiaries/${apiaryId}/hives/${hiveId}/diary/${entryId}`);
