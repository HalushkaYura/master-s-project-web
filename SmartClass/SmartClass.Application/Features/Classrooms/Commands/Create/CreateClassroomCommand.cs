using MediatR;
using SmartClass.Application.Contracts.Classrooms;

namespace SmartClass.Application.Features.Classrooms.Create;

public record CreateClassroomCommand(
    Guid OwnerId,
    string Title,
    string? Section,
    string? Description
) : IRequest<Guid>;
