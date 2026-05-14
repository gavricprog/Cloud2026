namespace SmartApiary.Application.DTOs.Parcels;

public class ParcelDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public CropDto? CurrentCrop { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CropDto
{
    public Guid Id { get; set; }
    public string CropType { get; set; } = string.Empty;
    public string BloomingPeriod { get; set; } = string.Empty;
    public string? AdditionalInfo { get; set; }
}
