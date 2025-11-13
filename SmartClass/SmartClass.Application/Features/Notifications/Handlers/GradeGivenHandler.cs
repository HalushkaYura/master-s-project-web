using MediatR;
using SmartClass.Application.Abstractions;
using SmartClass.Application.Common.DomainEvents;
using System.Text.Json;

namespace SmartClass.Application.Features.Notifications.Handlers;

public sealed class GradeGivenHandler : INotificationHandler<GradeGivenEvent>
{
    private readonly INotificationService notificationService;

    public GradeGivenHandler(INotificationService notificationService)
    {
        this.notificationService = notificationService;
    }

    public async Task Handle(GradeGivenEvent notification, CancellationToken ct)
    {
        var payload = JsonSerializer.Serialize(new
        {
            type = "GradeGiven",
            assignmentId = notification.AssignmentId,
            submissionId = notification.SubmissionId,
            score = notification.Score
        });

        await notificationService.CreateAsync(notification.StudentId, "GradeGiven", payload, ct);
    }
}
