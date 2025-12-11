using MediatR;
using Microsoft.AspNetCore.Components.Forms;
using SmartClass.Application.Abstractions;
using SmartClass.Application.Common.DomainEvents;
using SmartClass.Domain.Entities;

namespace SmartClass.Application.Features.Assignments.Commands;

public sealed class CreateAssignmentHandler : IRequestHandler<CreateAssignmentCommand, Guid>
{
    private readonly IRepository<Assignment> assignmentRepository;
    private readonly ICurrentUser currentUser;
    private readonly IMediator mediator;
    public CreateAssignmentHandler(IRepository<Assignment> assignmentRepository, ICurrentUser currentUser, IMediator mediator)
    {
        this.assignmentRepository = assignmentRepository;
        this.currentUser = currentUser;
        this.mediator = mediator;
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
        await mediator.Publish(new AssignmentCreatedEvent(
                classroomId: assignment.ClassroomId,
                assignmentId: assignment.Id,
                createdBy: userId,
                title: assignment.Title
            ), ct);
        return assignment.Id;
    }
}
