using MediatR;
using SmartClass.Application.Abstractions;
using SmartClass.Domain.Entities;
using SmartClass.Domain.Enums;

namespace SmartClass.Application.Features.Submissions.Commands.SubmitAssignment;

public sealed class SubmitAssignmentHandler : IRequestHandler<SubmitAssignmentCommand, Guid>
{
    private readonly IRepository<Submission> submissionRepo;
    private readonly IRepository<Assignment> assignmentRepo;
    private readonly ICurrentUser currentUser;

    public SubmitAssignmentHandler(
        IRepository<Submission> submissionRepo,
        IRepository<Assignment> assignmentRepo,
        ICurrentUser currentUser)
    {
        this.submissionRepo = submissionRepo;
        this.assignmentRepo = assignmentRepo;
        this.currentUser = currentUser;
    }

    public async Task<Guid> Handle(SubmitAssignmentCommand request, CancellationToken ct)
    {
        var studentId = currentUser.UserId
            ?? throw new InvalidOperationException("User must be authenticated.");

        var assignment = await assignmentRepo.GetByKeyAsync(request.AssignmentId)
            ?? throw new InvalidOperationException("Assignment not found.");

        // Перевірити дедлайн/статус — базова логіка (розшириш за потреби)
        if (assignment.Status == "Closed")
            throw new InvalidOperationException("Assignment is closed.");
        if (assignment.DueAt.HasValue && assignment.DueAt.Value < DateTime.UtcNow && !assignment.AllowLate)
            throw new InvalidOperationException("Deadline exceeded.");

        // Заборона дубль-подачі (одна спроба)
        var existing = await submissionRepo.GetListAsync(s =>
            s.AssignmentId == request.AssignmentId && s.StudentId == studentId);
        if (existing.Any())
            throw new InvalidOperationException("Submission already exists for this assignment.");

        var submission = new Submission
        {
            Id = Guid.NewGuid(),
            AssignmentId = request.AssignmentId,
            StudentId = studentId,
            Status = SubmissionStatus.Submitted,
            SubmittedAt = DateTime.UtcNow
        };

        await submissionRepo.AddAsync(submission);
        await submissionRepo.SaveChangesAsync();

        return submission.Id;
    }
}
