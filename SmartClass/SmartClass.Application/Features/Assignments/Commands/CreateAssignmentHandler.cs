using MediatR;
using SmartClass.Application.Abstractions;
using SmartClass.Domain.Entities;

namespace SmartClass.Application.Features.Assignments.Commands;

public sealed class CreateAssignmentHandler : IRequestHandler<CreateAssignmentCommand, Guid>
{
    private readonly IRepository<Assignment> assignmentRepository;
    private readonly ICurrentUser currentUser;

    public CreateAssignmentHandler(IRepository<Assignment> assignmentRepository, ICurrentUser currentUser)
    {
        this.assignmentRepository = assignmentRepository;
        this.currentUser = currentUser;
    }

    public async Task<Guid> Handle(CreateAssignmentCommand request, CancellationToken ct)
    {
        var userId = currentUser.UserId ?? throw new InvalidOperationException("User must be authenticated.");

        var assignment = new Assignment
        {
            Id = Guid.NewGuid(),
            ClassroomId = request.ClassroomId,
            CreatedBy = userId,
            Title = request.Title,
            DescriptionHtml = request.DescriptionHtml,
            PointsMax = request.PointsMax,
            DueAt = request.DueAt,
            AllowLate = request.AllowLate,
            Status = "Draft"
        };

        await assignmentRepository.AddAsync(assignment);
        await assignmentRepository.SaveChangesAsync();

        return assignment.Id;
    }
}
