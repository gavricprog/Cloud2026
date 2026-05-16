import api from "./api";

export const registerDevice = (apiaryId, hiveId, serialNumber) =>
  api.post(`/api/apiaries/${apiaryId}/hives/${hiveId}/device/register`, { serialNumber });
