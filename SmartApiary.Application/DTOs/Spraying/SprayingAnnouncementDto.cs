namespace SmartApiary.Application.DTOs.Spraying;

public class SprayingAnnouncementDto
{
    public Guid Id { get; set; }
    public Guid ParcelId { get; set; }
    public string ParcelName { get; set; } = string.Empty;
    public DateTime PlannedStartTime { get; set; }
    public int DurationHours { get; set; }
    public string? SubstanceType { get; set; }
    public string Status { get; set; } = string.Empty;
    public int NotifiedBeekeeperCount { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateSprayingRequest
{
    public Guid ParcelId { get; set; }
    public DateTime PlannedStartTime { get; set; }
    public int DurationHours { get; set; }
    public string? SubstanceType { get; set; }
}

public class UpdateSprayingRequest
{
    public DateTime PlannedStartTime { get; set; }
    public int DurationHours { get; set; }
    public string? SubstanceType { get; set; }
}
