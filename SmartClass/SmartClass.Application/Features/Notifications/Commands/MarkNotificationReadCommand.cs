using MediatR;

namespace SmartClass.Application.Features.Notifications.Commands;

public sealed class MarkNotificationReadCommand : IRequest
{
    public Guid NotificationId { get; init; }
}
