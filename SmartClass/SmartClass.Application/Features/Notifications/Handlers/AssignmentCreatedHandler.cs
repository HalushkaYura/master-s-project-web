using MediatR;
using SmartClass.Application.Abstractions;
using SmartClass.Application.Common.DomainEvents;
using SmartClass.Domain.Entities;
using System.Text.Json;

namespace SmartClass.Application.Features.Notifications.Handlers;

public sealed class AssignmentCreatedHandler : INotificationHandler<AssignmentCreatedEvent>
{
    private readonly IRepository<ClassMember> memberRepo;
    private readonly INotificationService notificationService;

    public AssignmentCreatedHandler(IRepository<ClassMember> memberRepo, INotificationService notificationService)
    {
        this.memberRepo = memberRepo;
        this.notificationService = notificationService;
    }

    public async Task Handle(AssignmentCreatedEvent notification, CancellationToken ct)
    {
        // всім членам класу, окрім автора (teacher), розсилаємо
        var members = await memberRepo.GetListAsync(m => m.ClassroomId == notification.ClassroomId);
        var payload = JsonSerializer.Serialize(new
        {
            type = "AssignmentCreated",
            assignmentId = notification.AssignmentId,
            classroomId = notification.ClassroomId,
            title = notification.Title
        });

        foreach (var m in members.Where(x => x.UserId != notification.CreatedBy))
        {
            await notificationService.CreateAsync(m.UserId, "AssignmentCreated", payload, ct);
        }
    }
}
