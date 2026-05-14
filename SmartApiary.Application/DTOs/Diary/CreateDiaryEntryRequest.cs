namespace SmartApiary.Application.DTOs.Diary;

public class CreateDiaryEntryRequest
{
    public DateTime InspectionDate { get; set; }
    public string FloorColor { get; set; } = string.Empty;
    public int HoneyFrames { get; set; }
    public double HoneyAmount { get; set; }
    public int BroodFrames { get; set; }
    public bool QueenPresent { get; set; }
    public string? Notes { get; set; }
}
