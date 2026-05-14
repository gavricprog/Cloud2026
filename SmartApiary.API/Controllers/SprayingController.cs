using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartApiary.Application.DTOs.Spraying;
using SmartApiary.Application.Interfaces;
using SmartApiary.Domain.Entities;
using SmartApiary.Domain.Enums;
using SmartApiary.Infrastructure.Persistence;

namespace SmartApiary.API.Controllers;

[ApiController]
[Route("api/spraying")]
[Authorize(Roles = "Farmer")]
public class SprayingController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IEmailService _emailService;
    private readonly IConfiguration _config;

    public SprayingController(AppDbContext db, IEmailService emailService, IConfiguration config)
    {
        _db = db;
        _emailService = emailService;
        _config = config;
    }

    private Guid CurrentUserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var announcements = await _db.SprayingAnnouncements
            .Include(s => s.Parcel)
            .Where(s => s.Parcel.FarmerId == CurrentUserId)
            .ToListAsync();

        var result = announcements.Select(s => MapToDto(s));

        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateSprayingRequest request)
    {
        var parcel = await _db.Parcels.FirstOrDefaultAsync(p => p.Id == request.ParcelId && p.FarmerId == CurrentUserId);
        if (parcel == null) return NotFound(new { message = "Parcela nije pronađena." });

        var announcement = new SprayingAnnouncement
        {
            Id = Guid.NewGuid(),
            ParcelId = request.ParcelId,
            PlannedStartTime = request.PlannedStartTime,
            DurationHours = request.DurationHours,
            SubstanceType = request.SubstanceType,
            Status = SprayingStatus.Scheduled
        };

        _db.SprayingAnnouncements.Add(announcement);

        var notifiedCount = await NotifyNearbyBeekeepers(announcement, parcel, "Najava prskanja");
        announcement.NotifiedBeekeeperCount = notifiedCount;

        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = announcement.Id }, MapToDto(announcement, parcel.Name));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var announcement = await _db.SprayingAnnouncements
            .Include(s => s.Parcel)
            .FirstOrDefaultAsync(s => s.Id == id && s.Parcel.FarmerId == CurrentUserId);

        if (announcement == null) return NotFound();

        return Ok(MapToDto(announcement));
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateSprayingRequest request)
    {
        var announcement = await _db.SprayingAnnouncements
            .Include(s => s.Parcel)
            .FirstOrDefaultAsync(s => s.Id == id && s.Parcel.FarmerId == CurrentUserId);

        if (announcement == null) return NotFound();
        if (announcement.Status == SprayingStatus.Cancelled)
            return BadRequest(new { message = "Ne možete ažurirati otkazano prskanje." });

        announcement.PlannedStartTime = request.PlannedStartTime;
        announcement.DurationHours = request.DurationHours;
        announcement.SubstanceType = request.SubstanceType;
        announcement.UpdatedAt = DateTime.UtcNow;

        await NotifyNearbyBeekeepersRescheduled(announcement, announcement.Parcel);
        await _db.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Cancel(Guid id)
    {
        var announcement = await _db.SprayingAnnouncements
            .Include(s => s.Parcel)
            .FirstOrDefaultAsync(s => s.Id == id && s.Parcel.FarmerId == CurrentUserId);

        if (announcement == null) return NotFound();
        if (announcement.Status == SprayingStatus.Cancelled)
            return BadRequest(new { message = "Prskanje je već otkazano." });

        announcement.Status = SprayingStatus.Cancelled;
        announcement.UpdatedAt = DateTime.UtcNow;

        await NotifyNearbyBeeekeepersCancelled(announcement, announcement.Parcel);
        await _db.SaveChangesAsync();

        return NoContent();
    }

    [HttpGet("{id:guid}/notification-status")]
    public async Task<IActionResult> GetNotificationStatus(Guid id)
    {
        var announcement = await _db.SprayingAnnouncements
            .Include(s => s.Parcel)
            .FirstOrDefaultAsync(s => s.Id == id && s.Parcel.FarmerId == CurrentUserId);

        if (announcement == null) return NotFound();

        return Ok(new
        {
            announcement.Id,
            announcement.NotifiedBeekeeperCount,
            announcement.Status,
            announcement.PlannedStartTime
        });
    }

    [HttpGet("logs")]
    public async Task<IActionResult> GetLogs([FromQuery] Guid? parcelId, [FromQuery] DateTime? from, [FromQuery] DateTime? to)
    {
        var query = _db.SprayingLogs
            .Include(l => l.SprayingAnnouncement)
                .ThenInclude(a => a.Parcel)
            .Where(l => l.SprayingAnnouncement.Parcel.FarmerId == CurrentUserId);

        if (parcelId.HasValue)
            query = query.Where(l => l.SprayingAnnouncement.ParcelId == parcelId.Value);
        if (from.HasValue)
            query = query.Where(l => l.ActualStartTime >= from.Value);
        if (to.HasValue)
            query = query.Where(l => l.ActualEndTime <= to.Value);

        var logs = await query.OrderByDescending(l => l.ActualStartTime).ToListAsync();

        return Ok(logs.Select(l => new
        {
            l.Id,
            l.ParcelName,
            l.CropType,
            l.SubstanceUsed,
            l.ActualStartTime,
            l.ActualEndTime,
            l.WindSpeed,
            l.Precipitation
        }));
    }

    private static SprayingAnnouncementDto MapToDto(SprayingAnnouncement s, string? parcelName = null) => new()
    {
        Id = s.Id,
        ParcelId = s.ParcelId,
        ParcelName = parcelName ?? s.Parcel?.Name ?? string.Empty,
        PlannedStartTime = s.PlannedStartTime,
        DurationHours = s.DurationHours,
        SubstanceType = s.SubstanceType,
        Status = s.Status.ToString(),
        NotifiedBeekeeperCount = s.NotifiedBeekeeperCount,
        CreatedAt = s.CreatedAt
    };

    private async Task<int> NotifyNearbyBeekeepers(SprayingAnnouncement announcement, Parcel parcel, string subject)
    {
        const double radiusKm = 5.0;
        var beekeepers = await GetBeekeepersInRadius(parcel.Latitude, parcel.Longitude, radiusKm);

        foreach (var beekeeper in beekeepers)
        {
            var alert = new Alert
            {
                Id = Guid.NewGuid(),
                Type = AlertType.Pesticide,
                Message = $"Poljoprivrednik je najavio prskanje parcele '{parcel.Name}' za {announcement.PlannedStartTime:dd.MM.yyyy HH:mm}. Trajanje: {announcement.DurationHours}h.",
                UserId = beekeeper.Id
            };
            _db.Alerts.Add(alert);

            await _emailService.SendAlertEmailAsync(
                beekeeper.Email, $"{beekeeper.FirstName} {beekeeper.LastName}",
                $"{subject}: {parcel.Name}",
                $"<p>Najavljeno prskanje pesticidima na parceli <strong>{parcel.Name}</strong>.</p><p>Datum: <strong>{announcement.PlannedStartTime:dd.MM.yyyy HH:mm}</strong>, trajanje: <strong>{announcement.DurationHours}h</strong>.</p>{(announcement.SubstanceType != null ? $"<p>Preparat: {announcement.SubstanceType}</p>" : "")}");
        }

        return beekeepers.Count;
    }

    private async Task NotifyNearbyBeeekeepersCancelled(SprayingAnnouncement announcement, Parcel parcel)
    {
        const double radiusKm = 5.0;
        var beekeepers = await GetBeekeepersInRadius(parcel.Latitude, parcel.Longitude, radiusKm);

        foreach (var beekeeper in beekeepers)
        {
            var alert = new Alert
            {
                Id = Guid.NewGuid(),
                Type = AlertType.SprayingCancelled,
                Message = $"Prskanje na parceli '{parcel.Name}' zakazano za {announcement.PlannedStartTime:dd.MM.yyyy HH:mm} je otkazano.",
                UserId = beekeeper.Id
            };
            _db.Alerts.Add(alert);

            await _emailService.SendAlertEmailAsync(
                beekeeper.Email, $"{beekeeper.FirstName} {beekeeper.LastName}",
                $"Otkazano prskanje: {parcel.Name}",
                $"<p>Prskanje na parceli <strong>{parcel.Name}</strong> zakazano za <strong>{announcement.PlannedStartTime:dd.MM.yyyy HH:mm}</strong> je <strong>otkazano</strong>.</p>");
        }
    }

    private async Task NotifyNearbyBeekeepersRescheduled(SprayingAnnouncement announcement, Parcel parcel)
    {
        const double radiusKm = 5.0;
        var beekeepers = await GetBeekeepersInRadius(parcel.Latitude, parcel.Longitude, radiusKm);

        foreach (var beekeeper in beekeepers)
        {
            var alert = new Alert
            {
                Id = Guid.NewGuid(),
                Type = AlertType.SprayingRescheduled,
                Message = $"Prskanje na parceli '{parcel.Name}' je pomereno na {announcement.PlannedStartTime:dd.MM.yyyy HH:mm}.",
                UserId = beekeeper.Id
            };
            _db.Alerts.Add(alert);

            await _emailService.SendAlertEmailAsync(
                beekeeper.Email, $"{beekeeper.FirstName} {beekeeper.LastName}",
                $"Promenjeno prskanje: {parcel.Name}",
                $"<p>Prskanje na parceli <strong>{parcel.Name}</strong> je pomereno na <strong>{announcement.PlannedStartTime:dd.MM.yyyy HH:mm}</strong>.</p>");
        }
    }

    private async Task<List<User>> GetBeekeepersInRadius(double lat, double lon, double radiusKm)
    {
        var allApiaries = await _db.Apiaries
            .Include(a => a.Beekeeper)
            .ToListAsync();

        return allApiaries
            .Where(a => HaversineDistance(lat, lon, a.Latitude, a.Longitude) <= radiusKm)
            .Select(a => a.Beekeeper)
            .DistinctBy(b => b.Id)
            .ToList();
    }

    private static double HaversineDistance(double lat1, double lon1, double lat2, double lon2)
    {
        const double R = 6371;
        var dLat = ToRad(lat2 - lat1);
        var dLon = ToRad(lon2 - lon1);
        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(ToRad(lat1)) * Math.Cos(ToRad(lat2)) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        return R * 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
    }

    private static double ToRad(double deg) => deg * Math.PI / 180;
}
