namespace SmartApiary.Application.DTOs.Hives;

public class HiveDto
{
    public Guid Id { get; set; }
    public string Label { get; set; } = string.Empty;
    public string HiveType { get; set; } = string.Empty;
    public string FrameColor { get; set; } = string.Empty;
    public int QueenAge { get; set; }
    public string? Notes { get; set; }
    public Guid ApiaryId { get; set; }
    public bool HasDevice { get; set; }
    public string? DeviceStatus { get; set; }
    public DateTime CreatedAt { get; set; }
}
