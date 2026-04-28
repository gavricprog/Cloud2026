using Microsoft.EntityFrameworkCore;
using SmartApiary.Api.Data;
using SmartApiary.Api.DTOs;
using SmartApiary.Api.Models;

namespace SmartApiary.Api.Services;

public sealed class TelemetryService(SmartApiaryDbContext db) : ITelemetryService
{
    public async Task<TelemetryDto> CreateAsync(TelemetryCreateDto dto, CancellationToken ct)
    {
        var entity = new Telemetry
        {
            Id = Guid.NewGuid(),
            HiveId = dto.HiveId,
            Timestamp = dto.Timestamp == default ? DateTimeOffset.UtcNow : dto.Timestamp,
            Weight = dto.Weight,
            Temperature = dto.Temperature,
            Humidity = dto.Humidity,
            BatteryLevel = dto.BatteryLevel,
        };

        db.Telemetry.Add(entity);
        await db.SaveChangesAsync(ct);

        return ToDto(entity);
    }

    public async Task<IReadOnlyList<TelemetryDto>> GetForHiveAsync(string hiveId, CancellationToken ct)
    {
        return await db.Telemetry
            .AsNoTracking()
            .Where(x => x.HiveId == hiveId)
            .OrderBy(x => x.Timestamp)
            .Select(x => ToDto(x))
            .ToListAsync(ct);
    }

    private static TelemetryDto ToDto(Telemetry x) => new()
    {
        Id = x.Id,
        HiveId = x.HiveId,
        Timestamp = x.Timestamp,
        Weight = x.Weight,
        Temperature = x.Temperature,
        Humidity = x.Humidity,
        BatteryLevel = x.BatteryLevel,
    };
}

