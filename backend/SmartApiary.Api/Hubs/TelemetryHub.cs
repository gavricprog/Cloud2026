using Microsoft.AspNetCore.SignalR;

namespace SmartApiary.Api.Hubs;

// Clients can subscribe to per-hive groups to get only the telemetry they care about.
public sealed class TelemetryHub : Hub
{
    public Task SubscribeHive(string hiveId) =>
        Groups.AddToGroupAsync(Context.ConnectionId, GroupName(hiveId));

    public Task UnsubscribeHive(string hiveId) =>
        Groups.RemoveFromGroupAsync(Context.ConnectionId, GroupName(hiveId));

    public static string GroupName(string hiveId) => $"hive:{hiveId}";
}

