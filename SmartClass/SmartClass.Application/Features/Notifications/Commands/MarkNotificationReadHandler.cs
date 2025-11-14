using MediatR;
using SmartClass.Application.Abstractions;
using SmartClass.Domain.Entities;

namespace SmartClass.Application.Features.Notifications.Commands;

public sealed class MarkNotificationReadHandler : IRequestHandler<MarkNotificationReadCommand>
{
    private readonly IRepository<Notification> repo;
    private readonly ICurrentUserService currentUser;

    public MarkNotificationReadHandler(IRepository<Notification> repo, ICurrentUserService currentUser)
    {
        this.repo = repo;
        this.currentUser = currentUser;
    }

    public async Task Handle(MarkNotificationReadCommand request, CancellationToken ct)
    {
        var userId = currentUser.UserId ?? throw new InvalidOperationException("User must be authenticated.");

        var n = await repo.GetByKeyAsync(request.NotificationId)
            ?? throw new InvalidOperationException("Notification not found.");

        if (n.UserId != userId) throw new InvalidOperationException("Forbidden.");

        n.IsRead = true;
        await repo.UpdateAsync(n);
        await repo.SaveChangesAsync();
    }
}
