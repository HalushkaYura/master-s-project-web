using MediatR;
using SmartClass.Application.Abstractions;
using SmartClass.Application.Features.Classrooms.Specifications;
using SmartClass.Domain.Entities;


namespace SmartClass.Application.Features.Notifications.Commands;

public sealed class MarkAllNotificationsReadHandler : IRequestHandler<MarkAllNotificationsReadCommand>
{
    private readonly IRepository<Notification> repo;
    private readonly ICurrentUserService currentUser;

    public MarkAllNotificationsReadHandler(IRepository<Notification> repo, ICurrentUserService currentUser)
    {
        this.repo = repo;
        this.currentUser = currentUser;
    }

    public async Task Handle(MarkAllNotificationsReadCommand request, CancellationToken ct)
    {
        var userId = currentUser.UserId ?? throw new InvalidOperationException("User must be authenticated.");

        var list = await repo.GetListAsync(n => n.UserId == userId && !n.IsRead);
        foreach (var n in list) n.IsRead = true;

        await repo.SaveChangesAsync();
    }
}
