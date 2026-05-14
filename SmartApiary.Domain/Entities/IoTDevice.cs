using SmartApiary.Domain.Enums;

namespace SmartApiary.Domain.Entities;

public class IoTDevice
{
    public Guid Id { get; set; }
    public string SerialNumber { get; set; } = string.Empty;
    public string? UniqueDeviceId { get; set; }
    public string? DeviceAccessToken { get; set; }
    public DeviceStatus Status { get; set; } = DeviceStatus.Registered;
    public bool BatteryAlertSent { get; set; }
    public Guid HiveId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Hive Hive { get; set; } = null!;
    public ICollection<TelemetryReading> TelemetryReadings { get; set; } = new List<TelemetryReading>();
}
