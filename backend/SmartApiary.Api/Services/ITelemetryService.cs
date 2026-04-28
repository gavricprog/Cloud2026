using SmartApiary.Api.DTOs;

namespace SmartApiary.Api.Services;

public interface ITelemetryService
{
    Task<TelemetryDto> CreateAsync(TelemetryCreateDto dto, CancellationToken ct);
    Task<IReadOnlyList<TelemetryDto>> GetForHiveAsync(string hiveId, CancellationToken ct);
}

