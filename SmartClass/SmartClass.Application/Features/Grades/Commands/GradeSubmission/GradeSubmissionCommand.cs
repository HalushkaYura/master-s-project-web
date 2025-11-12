using MediatR;

namespace SmartClass.Application.Features.Grades.Commands.GradeSubmission;

public sealed class GradeSubmissionCommand : IRequest<Guid>
{
    public Guid SubmissionId { get; init; }
    public decimal Score { get; init; }
    public string? FeedbackHtml { get; init; }
}
