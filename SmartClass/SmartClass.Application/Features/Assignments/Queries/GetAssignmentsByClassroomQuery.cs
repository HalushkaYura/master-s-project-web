using MediatR;
using SmartClass.Application.Contracts.Assignments;

namespace SmartClass.Application.Features.Assignments.Queries;

public sealed class GetAssignmentsByClassroomQuery : IRequest<IReadOnlyList<AssignmentDto>>
{
    public Guid ClassroomId { get; }

    public GetAssignmentsByClassroomQuery(Guid classroomId)
    {
        ClassroomId = classroomId;
    }
}
