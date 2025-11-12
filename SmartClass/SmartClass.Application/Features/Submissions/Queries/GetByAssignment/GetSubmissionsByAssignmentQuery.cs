using MediatR;

namespace SmartClass.Application.Features.Submissions.Queries.GetByAssignment;

public sealed class GetSubmissionsByAssignmentQuery : IRequest<IReadOnlyList<SubmissionViewDto>>
{
    public Guid AssignmentId { get; }
    public GetSubmissionsByAssignmentQuery(Guid assignmentId) => AssignmentId = assignmentId;
}
