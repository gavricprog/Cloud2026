import api from "./api";

export const getUserSettings = () => api.get("/api/users/me/settings");
export const updateUserSettings = (data) => api.put("/api/users/me/settings", data);
