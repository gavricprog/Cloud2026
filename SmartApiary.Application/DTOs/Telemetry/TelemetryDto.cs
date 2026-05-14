namespace SmartApiary.Application.DTOs.Telemetry;

public class TelemetryDto
{
    public Guid Id { get; set; }
    public Guid HiveId { get; set; }
    public double Weight { get; set; }
    public double Humidity { get; set; }
    public double InternalTemperature { get; set; }
    public double BatteryLevel { get; set; }
    public DateTime RecordedAt { get; set; }
}

public class DailyNectarDto
{
    public DateTime Date { get; set; }
    public double Delta { get; set; }
}

public class TelemetryChartDto
{
    public List<DailyNectarDto> DailyNectar { get; set; } = new();
    public List<TelemetryDto> TemperatureHumidity { get; set; } = new();
    public TelemetryDto? LatestReading { get; set; }
}
