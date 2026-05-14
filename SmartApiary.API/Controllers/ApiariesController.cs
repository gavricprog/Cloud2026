using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartApiary.Application.DTOs.Apiaries;
using SmartApiary.Domain.Entities;
using SmartApiary.Infrastructure.Persistence;

namespace SmartApiary.API.Controllers;

[ApiController]
[Route("api/apiaries")]
[Authorize(Roles = "Beekeeper")]
public class ApiariesController : ControllerBase
{
    private readonly AppDbContext _db;

    public ApiariesController(AppDbContext db)
    {
        _db = db;
    }

    private Guid CurrentUserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var apiaries = await _db.Apiaries
            .Where(a => a.BeekeeperId == CurrentUserId)
            .Select(a => new ApiaryDto
            {
                Id = a.Id,
                Name = a.Name,
                Latitude = a.Latitude,
                Longitude = a.Longitude,
                Description = a.Description,
                ImageUrl = a.ImageUrl,
                ThumbnailUrl = a.ThumbnailUrl,
                HiveCount = a.Hives.Count,
                CreatedAt = a.CreatedAt
            })
            .ToListAsync();

        return Ok(apiaries);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var apiary = await _db.Apiaries
            .Include(a => a.Hives)
            .FirstOrDefaultAsync(a => a.Id == id && a.BeekeeperId == CurrentUserId);

        if (apiary == null) return NotFound();

        return Ok(new ApiaryDto
        {
            Id = apiary.Id,
            Name = apiary.Name,
            Latitude = apiary.Latitude,
            Longitude = apiary.Longitude,
            Description = apiary.Description,
            ImageUrl = apiary.ImageUrl,
            ThumbnailUrl = apiary.ThumbnailUrl,
            HiveCount = apiary.Hives.Count,
            CreatedAt = apiary.CreatedAt
        });
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateApiaryRequest request)
    {
        var apiary = new Apiary
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Latitude = request.Latitude,
            Longitude = request.Longitude,
            Description = request.Description,
            BeekeeperId = CurrentUserId
        };

        _db.Apiaries.Add(apiary);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = apiary.Id }, new ApiaryDto
        {
            Id = apiary.Id,
            Name = apiary.Name,
            Latitude = apiary.Latitude,
            Longitude = apiary.Longitude,
            Description = apiary.Description,
            HiveCount = 0,
            CreatedAt = apiary.CreatedAt
        });
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateApiaryRequest request)
    {
        var apiary = await _db.Apiaries.FirstOrDefaultAsync(a => a.Id == id && a.BeekeeperId == CurrentUserId);
        if (apiary == null) return NotFound();

        apiary.Name = request.Name;
        apiary.Latitude = request.Latitude;
        apiary.Longitude = request.Longitude;
        apiary.Description = request.Description;
        await _db.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var apiary = await _db.Apiaries.FirstOrDefaultAsync(a => a.Id == id && a.BeekeeperId == CurrentUserId);
        if (apiary == null) return NotFound();

        _db.Apiaries.Remove(apiary);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpGet("{id:guid}/hives")]
    public async Task<IActionResult> GetHives(Guid id)
    {
        var apiary = await _db.Apiaries.FirstOrDefaultAsync(a => a.Id == id && a.BeekeeperId == CurrentUserId);
        if (apiary == null) return NotFound();

        var hives = await _db.Hives
            .Include(h => h.IoTDevice)
            .Where(h => h.ApiaryId == id)
            .Select(h => new Application.DTOs.Hives.HiveDto
            {
                Id = h.Id,
                Label = h.Label,
                HiveType = h.HiveType.ToString(),
                FrameColor = h.FrameColor,
                QueenAge = h.QueenAge,
                Notes = h.Notes,
                ApiaryId = h.ApiaryId,
                HasDevice = h.IoTDevice != null,
                DeviceStatus = h.IoTDevice != null ? h.IoTDevice.Status.ToString() : null,
                CreatedAt = h.CreatedAt
            })
            .ToListAsync();

        return Ok(hives);
    }
}
