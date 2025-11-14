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

    public async Task Handle(GradeGivenEvent e, CancellationToken ct)
    {
        var payload = JsonSerializer.Serialize(new
        {
            assignmentId = e.AssignmentId,
            submissionId = e.SubmissionId,
            score = e.Score
        });

        await notificationService.CreateAsync(
            userId: e.StudentId,
            type: "GradeGiven",
            payloadJson: payload,
            ct
        );
    }
}
