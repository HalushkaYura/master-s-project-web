using FluentValidation;

namespace SmartClass.Application.Features.Classrooms.Commands.Join;

public sealed class JoinClassroomValidator : AbstractValidator<JoinClassroomCommand>
{
    public JoinClassroomValidator()
    {
        RuleFor(x => x.JoinCode)
            .NotEmpty()
            .Length(6, 12)
            .Matches("^[A-Z0-9]+$")
            .WithMessage("Join code must contain only capital letters and digits.");
    }
}
