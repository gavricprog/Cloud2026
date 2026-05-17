using SmartApiary.Application.DTOs.Telemetry;

namespace SmartApiary.Application.Interfaces;

public interface ITelemetryTableService
{
    Task InsertReadingAsync(Guid hiveId, Guid deviceId, double weight, double humidity,
        double internalTemperature, double batteryLevel, DateTime recordedAt);

    Task<TelemetryDto?> GetLatestReadingAsync(Guid hiveId);

    Task<List<TelemetryDto>> GetReadingsAsync(Guid hiveId, DateTime from);

    Task<List<TelemetryDto>> GetReadingsForHivesAsync(IEnumerable<Guid> hiveIds, DateTime from);
}
