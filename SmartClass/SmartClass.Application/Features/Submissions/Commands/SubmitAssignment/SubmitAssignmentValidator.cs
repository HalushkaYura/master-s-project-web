using FluentValidation;

namespace SmartClass.Application.Features.Submissions.Commands.SubmitAssignment;

public sealed class SubmitAssignmentValidator : AbstractValidator<SubmitAssignmentCommand>
{
    public SubmitAssignmentValidator()
    {
        RuleFor(x => x.AssignmentId).NotEmpty();
    }
}
