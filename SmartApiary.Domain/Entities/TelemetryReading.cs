namespace SmartApiary.Domain.Entities;

public class TelemetryReading
{
    public Guid Id { get; set; }
    public Guid HiveId { get; set; }
    public Guid DeviceId { get; set; }
    public double Weight { get; set; }
    public double Humidity { get; set; }
    public double InternalTemperature { get; set; }
    public double BatteryLevel { get; set; }
    public DateTime RecordedAt { get; set; } = DateTime.UtcNow;

    public Hive Hive { get; set; } = null!;
    public IoTDevice Device { get; set; } = null!;
}
