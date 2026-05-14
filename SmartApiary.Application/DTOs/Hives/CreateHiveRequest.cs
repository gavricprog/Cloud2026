namespace SmartApiary.Application.DTOs.Hives;

public class CreateHiveRequest
{
    public string Label { get; set; } = string.Empty;
    public string HiveType { get; set; } = string.Empty;
    public string FrameColor { get; set; } = string.Empty;
    public int QueenAge { get; set; }
    public string? Notes { get; set; }
}
