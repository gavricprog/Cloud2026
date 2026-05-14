using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartApiary.Infrastructure.Persistence;

namespace SmartApiary.API.Controllers;

[ApiController]
[Route("api/alerts")]
[Authorize]
public class AlertsController : ControllerBase
{
    private readonly AppDbContext _db;

    public AlertsController(AppDbContext db)
    {
        _db = db;
    }

    private Guid CurrentUserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] bool unreadOnly = false)
    {
        var query = _db.Alerts.Where(a => a.UserId == CurrentUserId);
        if (unreadOnly) query = query.Where(a => !a.IsRead);

        var alerts = await query
            .OrderByDescending(a => a.CreatedAt)
            .Select(a => new
            {
                a.Id,
                Type = a.Type.ToString(),
                a.Message,
                a.IsRead,
                a.HiveId,
                a.ApiaryId,
                a.CreatedAt
            })
            .ToListAsync();

        return Ok(alerts);
    }

    [HttpPatch("{id:guid}/read")]
    public async Task<IActionResult> MarkRead(Guid id)
    {
        var alert = await _db.Alerts.FirstOrDefaultAsync(a => a.Id == id && a.UserId == CurrentUserId);
        if (alert == null) return NotFound();

        alert.IsRead = true;
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpPatch("read-all")]
    public async Task<IActionResult> MarkAllRead()
    {
        await _db.Alerts
            .Where(a => a.UserId == CurrentUserId && !a.IsRead)
            .ExecuteUpdateAsync(s => s.SetProperty(a => a.IsRead, true));

        return NoContent();
    }
}
