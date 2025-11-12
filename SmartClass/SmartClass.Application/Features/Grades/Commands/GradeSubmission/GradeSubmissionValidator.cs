using FluentValidation;

namespace SmartClass.Application.Features.Grades.Commands.GradeSubmission;

public sealed class GradeSubmissionValidator : AbstractValidator<GradeSubmissionCommand>
{
    public GradeSubmissionValidator()
    {
        RuleFor(x => x.SubmissionId).NotEmpty();
        RuleFor(x => x.Score).InclusiveBetween(0, 10000); // фактичну межу перевіримо проти PointsMax у handler
    }
}
