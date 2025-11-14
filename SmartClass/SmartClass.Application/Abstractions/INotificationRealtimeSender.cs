using SmartClass.Application.Contracts.Notifications;

namespace SmartClass.Application.Abstractions;

/// <summary>
/// Інтерфейс для push-сповіщень у реальному часі.
/// Реалізація — у шарі Web через SignalR.
/// </summary>
public interface INotificationRealtimeSender
{
    Task SendToUserAsync(Guid userId, NotificationDto dto, CancellationToken ct = default);
}
