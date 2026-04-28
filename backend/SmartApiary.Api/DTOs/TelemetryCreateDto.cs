namespace SmartApiary.Api.DTOs;

public sealed class TelemetryCreateDto
{
    public string HiveId { get; set; } = string.Empty;
    public DateTimeOffset Timestamp { get; set; }

    public decimal Weight { get; set; }
    public decimal Temperature { get; set; }
    public decimal Humidity { get; set; }
    public decimal BatteryLevel { get; set; }
}

