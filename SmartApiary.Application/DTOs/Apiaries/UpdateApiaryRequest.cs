namespace SmartApiary.Application.DTOs.Apiaries;

public class UpdateApiaryRequest
{
    public string Name { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string? Description { get; set; }
}
