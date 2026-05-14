using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartApiary.Application.DTOs.Diary;
using SmartApiary.Domain.Entities;
using SmartApiary.Infrastructure.Persistence;

namespace SmartApiary.API.Controllers;

[ApiController]
[Route("api/apiaries/{apiaryId:guid}/hives/{hiveId:guid}/diary")]
[Authorize(Roles = "Beekeeper")]
public class HiveDiaryController : ControllerBase
{
    private readonly AppDbContext _db;

    public HiveDiaryController(AppDbContext db)
    {
        _db = db;
    }

    private Guid CurrentUserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    private async Task<bool> OwnedHive(Guid apiaryId, Guid hiveId) =>
        await _db.Hives.AnyAsync(h =>
            h.Id == hiveId &&
            h.ApiaryId == apiaryId &&
            h.Apiary.BeekeeperId == CurrentUserId);

    [HttpGet]
    public async Task<IActionResult> GetAll(Guid apiaryId, Guid hiveId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        if (!await OwnedHive(apiaryId, hiveId)) return NotFound();

        var query = _db.HiveDiaryEntries
            .Where(e => e.HiveId == hiveId)
            .OrderByDescending(e => e.InspectionDate);

        var total = await query.CountAsync();
        var entries = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(e => new HiveDiaryEntryDto
            {
                Id = e.Id,
                HiveId = e.HiveId,
                InspectionDate = e.InspectionDate,
                FloorColor = e.FloorColor,
                HoneyFrames = e.HoneyFrames,
                HoneyAmount = e.HoneyAmount,
                BroodFrames = e.BroodFrames,
                QueenPresent = e.QueenPresent,
                Notes = e.Notes,
                CreatedAt = e.CreatedAt,
                UpdatedAt = e.UpdatedAt
            })
            .ToListAsync();

        return Ok(new { total, page, pageSize, data = entries });
    }

    [HttpPost]
    public async Task<IActionResult> Create(Guid apiaryId, Guid hiveId, [FromBody] CreateDiaryEntryRequest request)
    {
        if (!await OwnedHive(apiaryId, hiveId)) return NotFound();

        var entry = new HiveDiaryEntry
        {
            Id = Guid.NewGuid(),
            HiveId = hiveId,
            InspectionDate = request.InspectionDate,
            FloorColor = request.FloorColor,
            HoneyFrames = request.HoneyFrames,
            HoneyAmount = request.HoneyAmount,
            BroodFrames = request.BroodFrames,
            QueenPresent = request.QueenPresent,
            Notes = request.Notes
        };

        _db.HiveDiaryEntries.Add(entry);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { apiaryId, hiveId, id = entry.Id },
            MapToDto(entry));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid apiaryId, Guid hiveId, Guid id)
    {
        if (!await OwnedHive(apiaryId, hiveId)) return NotFound();

        var entry = await _db.HiveDiaryEntries.FirstOrDefaultAsync(e => e.Id == id && e.HiveId == hiveId);
        if (entry == null) return NotFound();

        return Ok(MapToDto(entry));
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid apiaryId, Guid hiveId, Guid id, [FromBody] CreateDiaryEntryRequest request)
    {
        if (!await OwnedHive(apiaryId, hiveId)) return NotFound();

        var entry = await _db.HiveDiaryEntries.FirstOrDefaultAsync(e => e.Id == id && e.HiveId == hiveId);
        if (entry == null) return NotFound();

        entry.InspectionDate = request.InspectionDate;
        entry.FloorColor = request.FloorColor;
        entry.HoneyFrames = request.HoneyFrames;
        entry.HoneyAmount = request.HoneyAmount;
        entry.BroodFrames = request.BroodFrames;
        entry.QueenPresent = request.QueenPresent;
        entry.Notes = request.Notes;
        entry.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid apiaryId, Guid hiveId, Guid id)
    {
        if (!await OwnedHive(apiaryId, hiveId)) return NotFound();

        var entry = await _db.HiveDiaryEntries.FirstOrDefaultAsync(e => e.Id == id && e.HiveId == hiveId);
        if (entry == null) return NotFound();

        _db.HiveDiaryEntries.Remove(entry);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    private static HiveDiaryEntryDto MapToDto(HiveDiaryEntry e) => new()
    {
        Id = e.Id,
        HiveId = e.HiveId,
        InspectionDate = e.InspectionDate,
        FloorColor = e.FloorColor,
        HoneyFrames = e.HoneyFrames,
        HoneyAmount = e.HoneyAmount,
        BroodFrames = e.BroodFrames,
        QueenPresent = e.QueenPresent,
        Notes = e.Notes,
        CreatedAt = e.CreatedAt,
        UpdatedAt = e.UpdatedAt
    };
}
