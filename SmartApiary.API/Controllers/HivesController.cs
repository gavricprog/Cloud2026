using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartApiary.Application.DTOs.Hives;
using SmartApiary.Domain.Entities;
using SmartApiary.Domain.Enums;
using SmartApiary.Infrastructure.Persistence;

namespace SmartApiary.API.Controllers;

[ApiController]
[Route("api/apiaries/{apiaryId:guid}/hives")]
[Authorize(Roles = "Beekeeper")]
public class HivesController : ControllerBase
{
    private readonly AppDbContext _db;

    public HivesController(AppDbContext db)
    {
        _db = db;
    }

    private Guid CurrentUserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    private async Task<bool> OwnedApiary(Guid apiaryId) =>
        await _db.Apiaries.AnyAsync(a => a.Id == apiaryId && a.BeekeeperId == CurrentUserId);

    [HttpPost]
    public async Task<IActionResult> Create(Guid apiaryId, [FromBody] CreateHiveRequest request)
    {
        if (!await OwnedApiary(apiaryId)) return NotFound();

        if (!Enum.TryParse<HiveType>(request.HiveType, ignoreCase: true, out var hiveType))
            return BadRequest(new { message = "Nevažeći tip košnice." });

        var hive = new Hive
        {
            Id = Guid.NewGuid(),
            Label = request.Label,
            HiveType = hiveType,
            FrameColor = request.FrameColor,
            QueenAge = request.QueenAge,
            Notes = request.Notes,
            ApiaryId = apiaryId
        };

        _db.Hives.Add(hive);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { apiaryId, id = hive.Id }, new HiveDto
        {
            Id = hive.Id,
            Label = hive.Label,
            HiveType = hive.HiveType.ToString(),
            FrameColor = hive.FrameColor,
            QueenAge = hive.QueenAge,
            Notes = hive.Notes,
            ApiaryId = hive.ApiaryId,
            HasDevice = false,
            CreatedAt = hive.CreatedAt
        });
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid apiaryId, Guid id)
    {
        if (!await OwnedApiary(apiaryId)) return NotFound();

        var hive = await _db.Hives
            .Include(h => h.IoTDevice)
            .FirstOrDefaultAsync(h => h.Id == id && h.ApiaryId == apiaryId);

        if (hive == null) return NotFound();

        return Ok(new HiveDto
        {
            Id = hive.Id,
            Label = hive.Label,
            HiveType = hive.HiveType.ToString(),
            FrameColor = hive.FrameColor,
            QueenAge = hive.QueenAge,
            Notes = hive.Notes,
            ApiaryId = hive.ApiaryId,
            HasDevice = hive.IoTDevice != null,
            DeviceStatus = hive.IoTDevice?.Status.ToString(),
            CreatedAt = hive.CreatedAt
        });
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid apiaryId, Guid id, [FromBody] UpdateHiveRequest request)
    {
        if (!await OwnedApiary(apiaryId)) return NotFound();

        var hive = await _db.Hives.FirstOrDefaultAsync(h => h.Id == id && h.ApiaryId == apiaryId);
        if (hive == null) return NotFound();

        if (!Enum.TryParse<HiveType>(request.HiveType, ignoreCase: true, out var hiveType))
            return BadRequest(new { message = "Nevažeći tip košnice." });

        hive.Label = request.Label;
        hive.HiveType = hiveType;
        hive.FrameColor = request.FrameColor;
        hive.QueenAge = request.QueenAge;
        hive.Notes = request.Notes;
        await _db.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid apiaryId, Guid id)
    {
        if (!await OwnedApiary(apiaryId)) return NotFound();

        var hive = await _db.Hives.FirstOrDefaultAsync(h => h.Id == id && h.ApiaryId == apiaryId);
        if (hive == null) return NotFound();

        _db.Hives.Remove(hive);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
