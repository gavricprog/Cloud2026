namespace SmartApiary.Application.DTOs.Apiaries;

public class ApiaryDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public string? ThumbnailUrl { get; set; }
    public int HiveCount { get; set; }
    public DateTime CreatedAt { get; set; }
}
