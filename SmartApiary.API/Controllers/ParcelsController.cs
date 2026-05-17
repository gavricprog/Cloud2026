using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartApiary.Application.DTOs.Parcels;
using SmartApiary.Domain.Entities;
using SmartApiary.Infrastructure.Helpers;
using SmartApiary.Infrastructure.Persistence;

namespace SmartApiary.API.Controllers;

[ApiController]
[Route("api/parcels")]
[Authorize]
public class ParcelsController : ControllerBase
{
    private readonly AppDbContext _db;

    public ParcelsController(AppDbContext db)
    {
        _db = db;
    }

    private Guid CurrentUserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    [Authorize(Roles = "Farmer")]
    public async Task<IActionResult> GetAll()
    {
        var parcels = await _db.Parcels
            .Include(p => p.CurrentCrop)
            .Where(p => p.FarmerId == CurrentUserId)
            .Select(p => new ParcelDto
            {
                Id = p.Id,
                Name = p.Name,
                Latitude = p.Latitude,
                Longitude = p.Longitude,
                AreaHectares = p.AreaHectares,
                CurrentCrop = p.CurrentCrop == null ? null : new CropDto
                {
                    Id = p.CurrentCrop.Id,
                    CropType = p.CurrentCrop.CropType,
                    BloomingPeriod = p.CurrentCrop.BloomingPeriod,
                    AdditionalInfo = p.CurrentCrop.AdditionalInfo
                },
                CreatedAt = p.CreatedAt
            })
            .ToListAsync();

        return Ok(parcels);
    }

    [HttpGet("{id:guid}")]
    [Authorize(Roles = "Farmer")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var parcel = await _db.Parcels
            .Include(p => p.CurrentCrop)
            .FirstOrDefaultAsync(p => p.Id == id && p.FarmerId == CurrentUserId);

        if (parcel == null) return NotFound();

        return Ok(new ParcelDto
        {
            Id = parcel.Id,
            Name = parcel.Name,
            Latitude = parcel.Latitude,
            Longitude = parcel.Longitude,
            AreaHectares = parcel.AreaHectares,
            CurrentCrop = parcel.CurrentCrop == null ? null : new CropDto
            {
                Id = parcel.CurrentCrop.Id,
                CropType = parcel.CurrentCrop.CropType,
                BloomingPeriod = parcel.CurrentCrop.BloomingPeriod,
                AdditionalInfo = parcel.CurrentCrop.AdditionalInfo
            },
            CreatedAt = parcel.CreatedAt
        });
    }

    [HttpPost]
    [Authorize(Roles = "Farmer")]
    public async Task<IActionResult> Create([FromBody] CreateParcelRequest request)
    {
        var parcel = new Parcel
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Latitude = request.Latitude,
            Longitude = request.Longitude,
            AreaHectares = request.AreaHectares,
            FarmerId = CurrentUserId
        };

        _db.Parcels.Add(parcel);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = parcel.Id }, new ParcelDto
        {
            Id = parcel.Id,
            Name = parcel.Name,
            Latitude = parcel.Latitude,
            Longitude = parcel.Longitude,
            AreaHectares = parcel.AreaHectares,
            CreatedAt = parcel.CreatedAt
        });
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Farmer")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateParcelRequest request)
    {
        var parcel = await _db.Parcels.FirstOrDefaultAsync(p => p.Id == id && p.FarmerId == CurrentUserId);
        if (parcel == null) return NotFound();

        parcel.Name = request.Name;
        parcel.Latitude = request.Latitude;
        parcel.Longitude = request.Longitude;
        parcel.AreaHectares = request.AreaHectares;
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpPut("{id:guid}/crop")]
    [Authorize(Roles = "Farmer")]
    public async Task<IActionResult> SetCrop(Guid id, [FromBody] SetCropRequest request)
    {
        var parcel = await _db.Parcels
            .Include(p => p.CurrentCrop)
            .FirstOrDefaultAsync(p => p.Id == id && p.FarmerId == CurrentUserId);
        if (parcel == null) return NotFound();

        if (parcel.CurrentCrop != null)
            _db.Crops.Remove(parcel.CurrentCrop);

        var crop = new Crop
        {
            Id = Guid.NewGuid(),
            ParcelId = id,
            CropType = request.CropType,
            BloomingPeriod = request.BloomingPeriod,
            AdditionalInfo = request.AdditionalInfo
        };

        _db.Crops.Add(crop);
        await _db.SaveChangesAsync();

        return Ok(new CropDto
        {
            Id = crop.Id,
            CropType = crop.CropType,
            BloomingPeriod = crop.BloomingPeriod,
            AdditionalInfo = crop.AdditionalInfo
        });
    }

    [HttpDelete("{id:guid}/crop")]
    [Authorize(Roles = "Farmer")]
    public async Task<IActionResult> DeleteCrop(Guid id)
    {
        var parcel = await _db.Parcels
            .Include(p => p.CurrentCrop)
            .FirstOrDefaultAsync(p => p.Id == id && p.FarmerId == CurrentUserId);
        if (parcel == null) return NotFound();
        if (parcel.CurrentCrop == null) return NotFound(new { message = "Parcela nema kulturu." });

        _db.Crops.Remove(parcel.CurrentCrop);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpGet("/api/parcels/public")]
    [Authorize(Roles = "Beekeeper,Farmer")]
    public async Task<IActionResult> GetPublicParcels()
    {
        var parcels = await _db.Parcels
            .Include(p => p.CurrentCrop)
            .Include(p => p.Farmer)
            .Select(p => new
            {
                p.Id,
                p.Name,
                p.Latitude,
                p.Longitude,
                p.AreaHectares,
                Crop = p.CurrentCrop == null ? null : new
                {
                    p.CurrentCrop.CropType,
                    p.CurrentCrop.BloomingPeriod,
                    p.CurrentCrop.AdditionalInfo
                },
                OwnerName = $"{p.Farmer.FirstName} {p.Farmer.LastName}",
                OwnerPhone = p.Farmer.Phone
            })
            .ToListAsync();

        return Ok(parcels);
    }

    [HttpGet("nearby")]
    [Authorize(Roles = "Beekeeper")]
    public async Task<IActionResult> GetNearbyParcels([FromQuery] Guid? apiaryId, [FromQuery] double radiusKm = SpatialHelper.DefaultRadiusKm)
    {
        var apiaries = await _db.Apiaries
            .Where(a => a.BeekeeperId == CurrentUserId)
            .ToListAsync();

        if (apiaryId.HasValue)
            apiaries = apiaries.Where(a => a.Id == apiaryId.Value).ToList();

        if (apiaries.Count == 0)
            return Ok(Array.Empty<object>());

        var parcels = await _db.Parcels
            .Include(p => p.CurrentCrop)
            .Include(p => p.Farmer)
            .Where(p => p.CurrentCrop != null)
            .ToListAsync();

        var nearby = parcels
            .Where(p => apiaries.Any(a =>
                SpatialHelper.IsWithinRadiusKm(a.Latitude, a.Longitude, p.Latitude, p.Longitude, radiusKm)))
            .DistinctBy(p => p.Id)
            .Select(p => new
            {
                p.Id,
                p.Name,
                p.Latitude,
                p.Longitude,
                p.AreaHectares,
                Crop = new
                {
                    p.CurrentCrop!.CropType,
                    p.CurrentCrop.BloomingPeriod,
                    p.CurrentCrop.AdditionalInfo
                },
                OwnerName = $"{p.Farmer.FirstName} {p.Farmer.LastName}",
                OwnerPhone = p.Farmer.Phone
            })
            .ToList();

        return Ok(nearby);
    }
}
