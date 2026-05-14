import api from "./api";

export const getTelemetryCharts = (apiaryId, days = 7) =>
  api.get(`/api/telemetry/apiaries/${apiaryId}?days=${days}`);
