using SmartApiary.Domain.Enums;

namespace SmartApiary.Domain.Entities;

public class Hive
{
    public Guid Id { get; set; }
    public string Label { get; set; } = string.Empty;
    public HiveType HiveType { get; set; }
    public string FrameColor { get; set; } = string.Empty;
    public int QueenAge { get; set; }
    public string? Notes { get; set; }
    public Guid ApiaryId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Apiary Apiary { get; set; } = null!;
    public IoTDevice? IoTDevice { get; set; }
    public ICollection<HiveDiaryEntry> DiaryEntries { get; set; } = new List<HiveDiaryEntry>();
    public ICollection<TelemetryReading> TelemetryReadings { get; set; } = new List<TelemetryReading>();
}
