import api from "./api";

export const getAlerts = (unreadOnly = false) =>
  api.get(`/api/alerts${unreadOnly ? "?unreadOnly=true" : ""}`);
export const markRead = (id) => api.patch(`/api/alerts/${id}/read`);
export const markAllRead = () => api.patch("/api/alerts/read-all");
