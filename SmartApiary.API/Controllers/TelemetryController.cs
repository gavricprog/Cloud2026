using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartApiary.Application.DTOs.Devices;
using SmartApiary.Application.DTOs.Telemetry;
using SmartApiary.Application.Interfaces;
using SmartApiary.Domain.Entities;
using SmartApiary.Domain.Enums;
using SmartApiary.Infrastructure.Persistence;
using SmartApiary.API.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace SmartApiary.API.Controllers;

[ApiController]
[Route("api/telemetry")]
public class TelemetryController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IEmailService _emailService;
    private readonly ITelemetryTableService _tableService;
    private readonly IHubContext<TelemetryHub> _hub;

    public TelemetryController(AppDbContext db, IEmailService emailService,
        ITelemetryTableService tableService, IHubContext<TelemetryHub> hub)
    {
        _db = db;
        _emailService = emailService;
        _tableService = tableService;
        _hub = hub;
    }

    [HttpPost("ingest")]
    [AllowAnonymous]
    public async Task<IActionResult> Ingest([FromHeader(Name = "X-Device-Token")] string deviceToken,
        [FromBody] TelemetryRequest request)
    {
        var device = await _db.IoTDevices
            .Include(d => d.Hive)
                .ThenInclude(h => h.Apiary)
                    .ThenInclude(a => a.Beekeeper)
            .FirstOrDefaultAsync(d => d.DeviceAccessToken == deviceToken && d.Status == DeviceStatus.Paired);

        if (device == null)
            return Unauthorized(new { message = "Nevažeći device token." });

        // Čuvaj merenje u Azure Table Storage
        await _tableService.InsertReadingAsync(
            device.HiveId, device.Id,
            request.Weight, request.Humidity,
            request.InternalTemperature, request.BatteryLevel,
            request.RecordedAt);

        await CheckAnomalies(device, request);

        await _db.SaveChangesAsync();

        // SignalR real-time update
        var apiaryId = device.Hive.ApiaryId.ToString();
        await _hub.Clients.Group($"apiary-{apiaryId}").SendAsync("TelemetryUpdate", new
        {
            weight = request.Weight,
            humidity = request.Humidity,
            internalTemperature = request.InternalTemperature,
            batteryLevel = request.BatteryLevel,
            recordedAt = request.RecordedAt
        });

        return Ok(new { message = "Telemetrija primljena." });
    }

    [HttpGet("apiaries/{apiaryId:guid}")]
    [Authorize(Roles = "Beekeeper")]
    public async Task<IActionResult> GetChartData(Guid apiaryId, [FromQuery] int days = 7)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var apiary = await _db.Apiaries.FirstOrDefaultAsync(a => a.Id == apiaryId && a.BeekeeperId == userId);
        if (apiary == null) return NotFound();

        var from = DateTime.UtcNow.AddDays(-days);
        var hiveIds = await _db.Hives.Where(h => h.ApiaryId == apiaryId).Select(h => h.Id).ToListAsync();

        // Čitaj iz Azure Table Storage
        var readings = await _tableService.GetReadingsForHivesAsync(hiveIds, from);

        var latestReading = readings.MaxBy(t => t.RecordedAt);

        var dailyNectar = readings
            .GroupBy(t => t.RecordedAt.Date)
            .Select(g =>
            {
                var morning = g.Where(t => t.RecordedAt.Hour >= 7 && t.RecordedAt.Hour <= 9)
                               .MinBy(t => Math.Abs(t.RecordedAt.Hour - 8))
                               ?? g.MinBy(t => t.RecordedAt);
                var evening = g.Where(t => t.RecordedAt.Hour >= 19 && t.RecordedAt.Hour <= 21)
                               .MinBy(t => Math.Abs(t.RecordedAt.Hour - 20))
                               ?? g.MaxBy(t => t.RecordedAt);
                var delta = (morning != null && evening != null && morning.RecordedAt != evening.RecordedAt)
                    ? Math.Round(evening.Weight - morning.Weight, 2)
                    : 0;
                return new DailyNectarDto { Date = g.Key, Delta = delta };
            })
            .OrderBy(d => d.Date)
            .ToList();

        return Ok(new TelemetryChartDto
        {
            DailyNectar = dailyNectar,
            TemperatureHumidity = readings,
            LatestReading = latestReading
        });
    }

    private async Task CheckAnomalies(IoTDevice device, TelemetryRequest newReading)
    {
        var beekeeper = device.Hive.Apiary.Beekeeper;
        var fullName = $"{beekeeper.FirstName} {beekeeper.LastName}";

        // Čitaj prethodno merenje iz Table Storage
        var previousReading = await _tableService.GetLatestReadingAsync(device.HiveId);

        if (previousReading != null)
        {
            var weightDrop = previousReading.Weight - newReading.Weight;
            if (weightDrop >= beekeeper.WeightDropThreshold)
            {
                var alert = new Alert
                {
                    Id = Guid.NewGuid(),
                    Type = AlertType.Theft,
                    Message = $"Detektovan nagli pad težine od {weightDrop:F1}kg na košnici {device.Hive.Label} u pčelinjaku {device.Hive.Apiary.Name}.",
                    UserId = beekeeper.Id,
                    ApiaryId = device.Hive.ApiaryId,
                    HiveId = device.HiveId
                };
                _db.Alerts.Add(alert);

                await _emailService.SendAlertEmailAsync(
                    beekeeper.Email, fullName,
                    "Upozorenje: Nagli pad težine košnice",
                    $"<p>Detektovan je nagli pad težine od <strong>{weightDrop:F1}kg</strong> na košnici <strong>{device.Hive.Label}</strong> u pčelinjaku <strong>{device.Hive.Apiary.Name}</strong>.</p><p>Moguća krađa ili prevrtanje košnice!</p>");
            }
        }

        if (newReading.BatteryLevel < 15 && !device.BatteryAlertSent)
        {
            var alert = new Alert
            {
                Id = Guid.NewGuid(),
                Type = AlertType.BatteryLow,
                Message = $"Baterija na uređaju u košnici {device.Hive.Label} je ispod 15% ({newReading.BatteryLevel:F0}%).",
                UserId = beekeeper.Id,
                HiveId = device.HiveId
            };
            _db.Alerts.Add(alert);
            device.BatteryAlertSent = true;

            await _emailService.SendAlertEmailAsync(
                beekeeper.Email, fullName,
                "Upozorenje: Nivo baterije",
                $"<p>Baterija na pametnoj vagi u košnici <strong>{device.Hive.Label}</strong> je pala na <strong>{newReading.BatteryLevel:F0}%</strong>.</p><p>Zamenite bateriju što pre.</p>");
        }
        else if (newReading.BatteryLevel >= 15 && device.BatteryAlertSent)
        {
            device.BatteryAlertSent = false;
        }
    }
}
