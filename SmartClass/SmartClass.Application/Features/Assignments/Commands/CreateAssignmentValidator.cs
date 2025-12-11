using FluentValidation;

namespace SmartClass.Application.Features.Assignments.Commands;

public sealed class CreateAssignmentValidator : AbstractValidator<CreateAssignmentCommand>
{
    public CreateAssignmentValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.PointsMax)
            .InclusiveBetween(1, 1000);
    }
}
