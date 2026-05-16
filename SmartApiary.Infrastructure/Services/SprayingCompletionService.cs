using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using SmartApiary.Application.Interfaces;
using SmartApiary.Domain.Entities;
using SmartApiary.Domain.Enums;
using SmartApiary.Infrastructure.Persistence;

namespace SmartApiary.Infrastructure.Services;

public class SprayingCompletionService : ISprayingCompletionService
{
    private readonly AppDbContext _db;
    private readonly IConfiguration _config;

    public SprayingCompletionService(AppDbContext db, IConfiguration config)
    {
        _db = db;
        _config = config;
    }

    public async Task<int> CompleteExpiredAnnouncementsAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var expired = await _db.SprayingAnnouncements
            .Include(s => s.Parcel)
                .ThenInclude(p => p.CurrentCrop)
            .Where(s => s.Status == SprayingStatus.Scheduled &&
                        s.PlannedStartTime.AddHours(s.DurationHours) <= now &&
                        s.SprayingLog == null)
            .ToListAsync(cancellationToken);

        foreach (var announcement in expired)
        {
            var (wind, precipitation) = await FetchWeatherAsync(
                announcement.Parcel.Latitude,
                announcement.Parcel.Longitude,
                announcement.PlannedStartTime);

            var log = new SprayingLog
            {
                Id = Guid.NewGuid(),
                SprayingAnnouncementId = announcement.Id,
                ActualStartTime = announcement.PlannedStartTime,
                ActualEndTime = announcement.PlannedStartTime.AddHours(announcement.DurationHours),
                ParcelName = announcement.Parcel.Name,
                CropType = announcement.Parcel.CurrentCrop?.CropType,
                SubstanceUsed = announcement.SubstanceType,
                WindSpeed = wind,
                Precipitation = precipitation
            };

            announcement.Status = SprayingStatus.Completed;
            announcement.UpdatedAt = now;
            _db.SprayingLogs.Add(log);
        }

        if (expired.Count > 0)
            await _db.SaveChangesAsync(cancellationToken);

        return expired.Count;
    }

    private async Task<(double Wind, double Precipitation)> FetchWeatherAsync(double lat, double lon, DateTime when)
    {
        var apiKey = _config["OpenWeatherMap:ApiKey"];
        if (string.IsNullOrWhiteSpace(apiKey) || apiKey.StartsWith("YOUR_", StringComparison.OrdinalIgnoreCase))
            return (0, 0);

        try
        {
            using var http = new HttpClient();
            var url =
                $"https://api.openweathermap.org/data/2.5/weather?lat={lat}&lon={lon}&appid={apiKey}&units=metric";
            var response = await http.GetFromJsonAsync<OpenWeatherResponse>(url);
            if (response == null) return (0, 0);
            var wind = response.Wind?.Speed ?? 0;
            var rain = response.Rain?.OneHour ?? response.Rain?.ThreeHours ?? 0;
            return (wind, rain);
        }
        catch
        {
            return (0, 0);
        }
    }

    private sealed class OpenWeatherResponse
    {
        public WindData? Wind { get; set; }
        public RainData? Rain { get; set; }
    }

    private sealed class WindData
    {
        public double Speed { get; set; }
    }

    private sealed class RainData
    {
        [System.Text.Json.Serialization.JsonPropertyName("1h")]
        public double? OneHour { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("3h")]
        public double? ThreeHours { get; set; }
    }
}
