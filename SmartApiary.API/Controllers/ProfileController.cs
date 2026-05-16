using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartApiary.Application.DTOs.Users;
using SmartApiary.Infrastructure.Persistence;

namespace SmartApiary.API.Controllers;

[ApiController]
[Route("api/users/me")]
[Authorize]
public class ProfileController : ControllerBase
{
    private readonly AppDbContext _db;

    public ProfileController(AppDbContext db)
    {
        _db = db;
    }

    private Guid CurrentUserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet("settings")]
    [Authorize(Roles = "Beekeeper")]
    public async Task<IActionResult> GetSettings()
    {
        var user = await _db.Users.FindAsync(CurrentUserId);
        if (user == null) return NotFound();

        return Ok(new UserSettingsDto { WeightDropThreshold = user.WeightDropThreshold });
    }

    [HttpPut("settings")]
    [Authorize(Roles = "Beekeeper")]
    public async Task<IActionResult> UpdateSettings([FromBody] UpdateUserSettingsRequest request)
    {
        if (request.WeightDropThreshold <= 0 || request.WeightDropThreshold > 100)
            return BadRequest(new { message = "Prag težine mora biti između 0.1 i 100 kg." });

        var user = await _db.Users.FindAsync(CurrentUserId);
        if (user == null) return NotFound();

        user.WeightDropThreshold = request.WeightDropThreshold;
        await _db.SaveChangesAsync();

        return Ok(new UserSettingsDto { WeightDropThreshold = user.WeightDropThreshold });
    }
}
