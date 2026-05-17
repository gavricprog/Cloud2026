namespace SmartApiary.Domain.Entities;

public class Parcel
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public double? AreaHectares { get; set; }
    public Guid FarmerId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public User Farmer { get; set; } = null!;
    public Crop? CurrentCrop { get; set; }
    public ICollection<SprayingAnnouncement> SprayingAnnouncements { get; set; } = new List<SprayingAnnouncement>();
}
