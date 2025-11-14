using MediatR;
using SmartClass.Application.Abstractions;
using SmartClass.Domain.Entities;
using SmartClass.Application.Contracts.Assignments; // якщо триматимеш DTO тут — або видали

namespace SmartClass.Application.Features.Submissions.Queries.GetByAssignment;

public sealed class GetSubmissionsByAssignmentHandler
    : IRequestHandler<GetSubmissionsByAssignmentQuery, IReadOnlyList<SubmissionViewDto>>
{
    private readonly IRepository<Submission> submissionRepo;
    private readonly IRepository<Assignment> assignmentRepo;
    private readonly IRepository<Classroom> classroomRepo;
    private readonly IRepository<Grade> gradeRepo;
    private readonly ICurrentUserService currentUser;

    public GetSubmissionsByAssignmentHandler(
        IRepository<Submission> submissionRepo,
        IRepository<Assignment> assignmentRepo,
        IRepository<Classroom> classroomRepo,
        IRepository<Grade> gradeRepo,
        ICurrentUserService currentUser)
    {
        this.submissionRepo = submissionRepo;
        this.assignmentRepo = assignmentRepo;
        this.classroomRepo = classroomRepo;
        this.gradeRepo = gradeRepo;
        this.currentUser = currentUser;
    }

    public async Task<IReadOnlyList<SubmissionViewDto>> Handle(GetSubmissionsByAssignmentQuery request, CancellationToken ct)
    {
        var teacherId = currentUser.UserId
            ?? throw new InvalidOperationException("User must be authenticated.");

        var assignment = await assignmentRepo.GetByKeyAsync(request.AssignmentId)
            ?? throw new InvalidOperationException("Assignment not found.");

        var classroom = await classroomRepo.GetByKeyAsync(assignment.ClassroomId)
            ?? throw new InvalidOperationException("Classroom not found.");

        if (classroom.OwnerId != teacherId)
            throw new InvalidOperationException("Only the class owner can view submissions.");

        var submissions = await submissionRepo.GetListAsync(s => s.AssignmentId == request.AssignmentId);

        // Підтягнемо оцінки пачкою:
        var submissionIds = submissions.Select(s => s.Id).ToArray();
        var grades = await gradeRepo.GetListAsync(g => submissionIds.Contains(g.SubmissionId));
        var gradeMap = grades.ToDictionary(g => g.SubmissionId, g => g);

        return submissions
            .Select(s => new SubmissionViewDto
            {
                SubmissionId = s.Id,
                StudentId = s.StudentId,
                SubmittedAt = s.SubmittedAt,
                Status = s.Status.ToString(),
                Score = gradeMap.TryGetValue(s.Id, out var gr) ? gr.Score : null,
                FeedbackHtml = gradeMap.TryGetValue(s.Id, out var gr2) ? gr2.FeedbackHtml : null
            })
            .ToList();
    }
}
