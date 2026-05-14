namespace SmartApiary.Domain.Entities;

public class HiveDiaryEntry
{
    public Guid Id { get; set; }
    public Guid HiveId { get; set; }
    public DateTime InspectionDate { get; set; }
    public string FloorColor { get; set; } = string.Empty;
    public int HoneyFrames { get; set; }
    public double HoneyAmount { get; set; }
    public int BroodFrames { get; set; }
    public bool QueenPresent { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public Hive Hive { get; set; } = null!;
}
