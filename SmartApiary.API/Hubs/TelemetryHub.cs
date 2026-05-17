using Microsoft.AspNetCore.SignalR;

namespace SmartApiary.API.Hubs;

public class TelemetryHub : Hub
{
    public async Task JoinApiary(string apiaryId) =>
        await Groups.AddToGroupAsync(Context.ConnectionId, $"apiary-{apiaryId}");

    public async Task LeaveApiary(string apiaryId) =>
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"apiary-{apiaryId}");
}
