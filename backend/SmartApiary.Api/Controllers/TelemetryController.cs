using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using SmartApiary.Api.DTOs;
using SmartApiary.Api.Hubs;
using SmartApiary.Api.Services;

namespace SmartApiary.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class TelemetryController(
    ITelemetryService telemetry,
    IHubContext<TelemetryHub> hub
) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<TelemetryDto>> Create([FromBody] TelemetryCreateDto dto, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(dto.HiveId))
            return BadRequest("HiveId is required.");

        var created = await telemetry.CreateAsync(dto, ct);

        await hub.Clients
            .Group(TelemetryHub.GroupName(created.HiveId))
            .SendAsync("telemetryInserted", created, ct);

        return CreatedAtAction(nameof(GetForHive), new { hiveId = created.HiveId }, created);
    }

    [HttpGet("{hiveId}")]
    public async Task<ActionResult<IReadOnlyList<TelemetryDto>>> GetForHive([FromRoute] string hiveId, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(hiveId))
            return BadRequest("hiveId is required.");

        var data = await telemetry.GetForHiveAsync(hiveId, ct);
        return Ok(data);
    }
}

