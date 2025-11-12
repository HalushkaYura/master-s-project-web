using MediatR;

namespace SmartClass.Application.Features.Submissions.Commands.SubmitAssignment;

public sealed class SubmitAssignmentCommand : IRequest<Guid>
{
    public Guid AssignmentId { get; init; }
    // (пізніше думаю що додамо AnswerText / Files, зараз лишаємо мінімум)
}
