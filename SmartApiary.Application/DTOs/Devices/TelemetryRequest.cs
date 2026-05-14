namespace SmartApiary.Application.DTOs.Devices;

public class TelemetryRequest
{
    public double Weight { get; set; }
    public double Humidity { get; set; }
    public double InternalTemperature { get; set; }
    public double BatteryLevel { get; set; }
    public DateTime RecordedAt { get; set; } = DateTime.UtcNow;
}
