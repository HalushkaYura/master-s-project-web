using FluentValidation;

namespace SmartClass.Application.Features.Classrooms.Commands.Create;

public sealed class CreateClassroomValidator : AbstractValidator<CreateClassroomCommand>
{
    public CreateClassroomValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(120);
        RuleFor(x => x.Section).MaximumLength(60).When(x => x.Section != null);
        RuleFor(x => x.Description).MaximumLength(2000).When(x => x.Description != null);
    }
}
