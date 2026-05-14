using SmartApiary.Domain.Enums;

namespace SmartApiary.Domain.Entities;

public class SprayingAnnouncement
{
    public Guid Id { get; set; }
    public Guid ParcelId { get; set; }
    public DateTime PlannedStartTime { get; set; }
    public int DurationHours { get; set; }
    public string? SubstanceType { get; set; }
    public SprayingStatus Status { get; set; } = SprayingStatus.Scheduled;
    public int NotifiedBeekeeperCount { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public Parcel Parcel { get; set; } = null!;
    public SprayingLog? SprayingLog { get; set; }
}
