namespace SmartApiary.Domain.Entities;

public class SprayingLog
{
    public Guid Id { get; set; }
    public Guid SprayingAnnouncementId { get; set; }
    public DateTime ActualStartTime { get; set; }
    public DateTime ActualEndTime { get; set; }
    public string ParcelName { get; set; } = string.Empty;
    public string? CropType { get; set; }
    public string? SubstanceUsed { get; set; }
    public double WindSpeed { get; set; }
    public double Precipitation { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public SprayingAnnouncement SprayingAnnouncement { get; set; } = null!;
}
