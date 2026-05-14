namespace SmartApiary.Domain.Entities;

public class Crop
{
    public Guid Id { get; set; }
    public Guid ParcelId { get; set; }
    public string CropType { get; set; } = string.Empty;
    public string BloomingPeriod { get; set; } = string.Empty;
    public string? AdditionalInfo { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Parcel Parcel { get; set; } = null!;
}
