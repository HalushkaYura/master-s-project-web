using MediatR;

namespace SmartClass.Application.Features.Classrooms.Commands.Create;

public sealed class CreateClassroomCommand : IRequest<Guid>
{
    public string Title { get; init; } = string.Empty;
    public string? Section { get; init; }
    public string? Description { get; init; }
    // JoinCode НЕ обов’язково передавати; згенеруємо на бекенді
}
