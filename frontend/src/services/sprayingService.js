import api from "./api";

export const getAnnouncements = () => api.get("/api/spraying");

export const createAnnouncement = async (data) => {
    const response = await api.post("/api/spraying", data);

    if (response.data.weatherWarning) {
        alert(response.data.weatherWarning);
    }

    return response;
};

export const updateAnnouncement = (id, data) => api.put(`/api/spraying/${id}`, data);
export const cancelAnnouncement = (id) => api.delete(`/api/spraying/${id}`);
export const getNotificationStatus = (id) => api.get(`/api/spraying/${id}/notification-status`);
export const getSprayingLogs = (parcelId, from, to) => {
  const params = new URLSearchParams();
  if (parcelId) params.append("parcelId", parcelId);
  if (from) params.append("from", from);
  if (to) params.append("to", to);
  return api.get(`/api/spraying/logs?${params}`);
};

export const exportSprayingLogsPdf = (parcelId, from, to) => {
  const params = new URLSearchParams();
  if (parcelId) params.append("parcelId", parcelId);
  if (from) params.append("from", from);
  if (to) params.append("to", to);
  return api.get(`/api/spraying/logs/export?${params}`, { responseType: "blob" });
};
