namespace SmartApiary.Api.Models;

public sealed class Telemetry
{
    public Guid Id { get; set; }
    public string HiveId { get; set; } = string.Empty;
    public DateTimeOffset Timestamp { get; set; }

    public decimal Weight { get; set; }
    public decimal Temperature { get; set; }
    public decimal Humidity { get; set; }
    public decimal BatteryLevel { get; set; }
}

