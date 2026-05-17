using Azure.Data.Tables;
using Microsoft.Extensions.Configuration;
using SmartApiary.Application.DTOs.Telemetry;
using SmartApiary.Application.Interfaces;
using SmartApiary.Infrastructure.Persistence;

namespace SmartApiary.Infrastructure.Services;

public class TelemetryTableService : ITelemetryTableService
{
    private readonly TableClient _tableClient;

    public TelemetryTableService(IConfiguration config)
    {
        var connectionString = config["TableStorage:ConnectionString"] ?? "UseDevelopmentStorage=true";
        var tableName = config["TableStorage:TelemetryTableName"] ?? "TelemetryReadings";
        _tableClient = new TableClient(connectionString, tableName);
        _tableClient.CreateIfNotExists();
    }

    public async Task InsertReadingAsync(Guid hiveId, Guid deviceId, double weight, double humidity,
        double internalTemperature, double batteryLevel, DateTime recordedAt)
    {
        var entity = new TelemetryTableEntity
        {
            PartitionKey = hiveId.ToString(),
            RowKey = TelemetryTableEntity.BuildRowKey(recordedAt),
            DeviceId = deviceId.ToString(),
            Weight = weight,
            Humidity = humidity,
            InternalTemperature = internalTemperature,
            BatteryLevel = batteryLevel,
            RecordedAt = recordedAt
        };

        await _tableClient.AddEntityAsync(entity);
    }

    public async Task<TelemetryDto?> GetLatestReadingAsync(Guid hiveId)
    {
        var results = _tableClient
            .QueryAsync<TelemetryTableEntity>(
                filter: $"PartitionKey eq '{hiveId}'",
                maxPerPage: 1)
            .AsPages(pageSizeHint: 1);

        await foreach (var page in results)
        {
            var entity = page.Values.FirstOrDefault();
            if (entity != null) return MapToDto(entity);
            break;
        }

        return null;
    }

    public async Task<List<TelemetryDto>> GetReadingsAsync(Guid hiveId, DateTime from)
    {
        var fromUtc = from.ToUniversalTime();
        var rowKeyMax = TelemetryTableEntity.BuildRowKey(fromUtc).Split('_')[0];

        var readings = new List<TelemetryDto>();
        await foreach (var entity in _tableClient.QueryAsync<TelemetryTableEntity>(
            filter: $"PartitionKey eq '{hiveId}' and RowKey le '{rowKeyMax}'"))
        {
            if (entity.RecordedAt >= fromUtc)
                readings.Add(MapToDto(entity));
        }

        return readings.OrderBy(r => r.RecordedAt).ToList();
    }

    public async Task<List<TelemetryDto>> GetReadingsForHivesAsync(IEnumerable<Guid> hiveIds, DateTime from)
    {
        var tasks = hiveIds.Select(hiveId => GetReadingsAsync(hiveId, from));
        var results = await Task.WhenAll(tasks);
        return results.SelectMany(r => r).OrderBy(r => r.RecordedAt).ToList();
    }

    private static TelemetryDto MapToDto(TelemetryTableEntity e) => new()
    {
        Id = Guid.NewGuid(),
        HiveId = Guid.Parse(e.PartitionKey),
        Weight = e.Weight,
        Humidity = e.Humidity,
        InternalTemperature = e.InternalTemperature,
        BatteryLevel = e.BatteryLevel,
        RecordedAt = e.RecordedAt
    };
}
