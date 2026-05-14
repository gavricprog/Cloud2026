using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartApiary.Application.DTOs.Devices;
using SmartApiary.Domain.Entities;
using SmartApiary.Domain.Enums;
using SmartApiary.Infrastructure.Persistence;

namespace SmartApiary.API.Controllers;

[ApiController]
[Route("api/apiaries/{apiaryId:guid}/hives/{hiveId:guid}/device")]
public class DevicesController : ControllerBase
{
    private readonly AppDbContext _db;

    public DevicesController(AppDbContext db)
    {
        _db = db;
    }

    private Guid CurrentUserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpPost("register")]
    [Authorize(Roles = "Beekeeper")]
    public async Task<IActionResult> Register(Guid apiaryId, Guid hiveId, [FromBody] RegisterDeviceRequest request)
    {
        var owned = await _db.Apiaries.AnyAsync(a => a.Id == apiaryId && a.BeekeeperId == CurrentUserId);
        if (!owned) return NotFound();

        var hive = await _db.Hives
            .Include(h => h.IoTDevice)
            .FirstOrDefaultAsync(h => h.Id == hiveId && h.ApiaryId == apiaryId);
        if (hive == null) return NotFound();

        if (hive.IoTDevice != null)
            return Conflict(new { message = "Košnica već ima registrovan uređaj." });

        var serialRegex = new System.Text.RegularExpressions.Regex(@"^SA-\d{4}-\d{5}$");
        if (!serialRegex.IsMatch(request.SerialNumber))
            return BadRequest(new { message = "Serijski broj mora biti u formatu SA-YYYY-XXXXX." });

        var device = new IoTDevice
        {
            Id = Guid.NewGuid(),
            SerialNumber = request.SerialNumber,
            Status = DeviceStatus.Registered,
            HiveId = hiveId
        };

        _db.IoTDevices.Add(device);
        await _db.SaveChangesAsync();

        return Ok(new { deviceId = device.Id, status = device.Status.ToString() });
    }

    [HttpPost("/api/devices/handshake")]
    [AllowAnonymous]
    public async Task<IActionResult> Handshake([FromBody] DeviceHandshakeRequest request)
    {
        var device = await _db.IoTDevices.FirstOrDefaultAsync(d =>
            d.SerialNumber == request.SerialNumber &&
            d.Status == DeviceStatus.Registered);

        if (device == null)
            return NotFound(new { message = "Uređaj nije registrovan ili je već uparen." });

        var accessToken = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));

        device.UniqueDeviceId = request.UniqueDeviceId;
        device.DeviceAccessToken = accessToken;
        device.Status = DeviceStatus.Paired;
        await _db.SaveChangesAsync();

        return Ok(new { accessToken });
    }
}
