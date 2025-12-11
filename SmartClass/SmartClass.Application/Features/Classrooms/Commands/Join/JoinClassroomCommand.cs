using MediatR;

namespace SmartClass.Application.Features.Classrooms.Commands.Join;

public sealed class JoinClassroomCommand : IRequest<Guid>
{
    public string JoinCode { get; init; } = string.Empty;
}
