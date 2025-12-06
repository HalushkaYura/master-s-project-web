using MediatR;
using SmartClass.Application.Abstractions;
using SmartClass.Application.Contracts.Assignments;
using SmartClass.Domain.Entities;

namespace SmartClass.Application.Features.Assignments.Queries;

public sealed class GetAssignmentsByClassroomHandler
    : IRequestHandler<GetAssignmentsByClassroomQuery, IReadOnlyList<AssignmentDto>>
{
    private readonly IRepository<Assignment> repository;

    public GetAssignmentsByClassroomHandler(IRepository<Assignment> repository)
    {
        this.repository = repository;
    }

    public async Task<IReadOnlyList<AssignmentDto>> Handle(GetAssignmentsByClassroomQuery request, CancellationToken ct)
    {
        var items = await repository.GetListAsync(x => x.ClassroomId == request.ClassroomId);

        return items.Select(x => new AssignmentDto(
            x.Id,
            x.ClassroomId,
            x.CreatedBy,
            x.Title,
            x.DescriptionHtml,
            x.PointsMax,
            x.DueAt,
            x.AllowLate,
            x.Status
        )).ToList();
    }
}
