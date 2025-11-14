using Microsoft.AspNetCore.SignalR;
using SmartClass.Application.Abstractions;
using SmartClass.Application.Contracts.Notifications;
using SmartClass.Web.Hubs;

namespace SmartClass.Web.Services;

public sealed class NotificationRealtimeSender : INotificationRealtimeSender
{
    private readonly IHubContext<NotificationsHub> hub;

    public NotificationRealtimeSender(IHubContext<NotificationsHub> hub)
    {
        this.hub = hub;
    }

    public async Task SendToUserAsync(Guid userId, NotificationDto dto, CancellationToken ct = default)
    {
        // Шлемо через групу та через UserId для сумісності
        await hub.Clients.User(userId.ToString()).SendAsync("notification", dto, ct);
        await hub.Clients.Group($"user-{userId}").SendAsync("notification", dto, ct);
    }
}
