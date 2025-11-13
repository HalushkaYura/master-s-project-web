using MediatR;
using SmartClass.Application.Contracts.Notifications;

namespace SmartClass.Application.Features.Notifications.Queries;

public sealed class GetMyNotificationsQuery : IRequest<IReadOnlyList<NotificationDto>> { }
