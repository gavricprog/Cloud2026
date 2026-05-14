namespace SmartApiary.Application.DTOs.Parcels;

public class CreateParcelRequest
{
    public string Name { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
}

public class UpdateParcelRequest
{
    public string Name { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
}

public class SetCropRequest
{
    public string CropType { get; set; } = string.Empty;
    public string BloomingPeriod { get; set; } = string.Empty;
    public string? AdditionalInfo { get; set; }
}
