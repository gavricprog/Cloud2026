namespace SmartApiary.Domain.Entities;

public class Apiary
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public string? ThumbnailUrl { get; set; }
    public Guid BeekeeperId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public User Beekeeper { get; set; } = null!;
    public ICollection<Hive> Hives { get; set; } = new List<Hive>();
}
