using Azure;
using Azure.Data.Tables;

namespace SmartApiary.Infrastructure.Persistence;

public class TelemetryTableEntity : ITableEntity
{
    // PartitionKey = HiveId
    // RowKey = obrnuti timestamp (najnovija merenja dolaze prva)
    public string PartitionKey { get; set; } = string.Empty;
    public string RowKey { get; set; } = string.Empty;
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }

    public string DeviceId { get; set; } = string.Empty;
    public double Weight { get; set; }
    public double Humidity { get; set; }
    public double InternalTemperature { get; set; }
    public double BatteryLevel { get; set; }
    public DateTime RecordedAt { get; set; }

    public static string BuildRowKey(DateTime recordedAt) =>
        (DateTime.MaxValue.Ticks - recordedAt.ToUniversalTime().Ticks).ToString("D20")
        + "_" + Guid.NewGuid().ToString("N");
}
