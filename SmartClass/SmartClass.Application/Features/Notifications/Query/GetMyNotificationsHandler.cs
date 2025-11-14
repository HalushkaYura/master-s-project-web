using MediatR;
using SmartClass.Application.Abstractions;
using SmartClass.Application.Contracts.Notifications;
using SmartClass.Domain.Entities;

namespace SmartClass.Application.Features.Notifications.Queries;

public sealed class GetMyNotificationsHandler
    : IRequestHandler<GetMyNotificationsQuery, IReadOnlyList<NotificationDto>>
{
    private readonly IRepository<Notification> repo;
    private readonly ICurrentUserService currentUser;

    public GetMyNotificationsHandler(IRepository<Notification> repo, ICurrentUserService currentUser)
    {
        this.repo = repo;
        this.currentUser = currentUser;
    }

    public async Task<IReadOnlyList<NotificationDto>> Handle(GetMyNotificationsQuery request, CancellationToken ct)
    {
        var userId = currentUser.UserId ?? throw new InvalidOperationException("User must be authenticated.");

        var list = await repo.GetListAsync(n => n.UserId == userId,
            orderBy: q => q.OrderByDescending(x => x.CreatedAt));

        return list.Select(x => new NotificationDto
        {
            Id = x.Id,
            Type = x.Type,
            PayloadJson = x.PayloadJson,
            IsRead = x.IsRead,
            CreatedAt = x.CreatedAt
        }).ToList();
    }
}
