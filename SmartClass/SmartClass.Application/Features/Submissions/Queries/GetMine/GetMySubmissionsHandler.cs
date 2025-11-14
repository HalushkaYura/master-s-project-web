using MediatR;
using SmartClass.Application.Abstractions;
using SmartClass.Application.Contracts.Submissions;
using SmartClass.Domain.Entities;

namespace SmartClass.Application.Features.Submissions.Queries.GetMine;

public sealed class GetMySubmissionsHandler
    : IRequestHandler<GetMySubmissionsQuery, IReadOnlyList<MySubmissionDto>>
{
    private readonly IRepository<Submission> submissionRepo;
    private readonly IRepository<Assignment> assignmentRepo;
    private readonly IRepository<Grade> gradeRepo;
    private readonly ICurrentUserService currentUser;

    public GetMySubmissionsHandler(
        IRepository<Submission> submissionRepo,
        IRepository<Assignment> assignmentRepo,
        IRepository<Grade> gradeRepo,
        ICurrentUserService currentUser)
    {
        this.submissionRepo = submissionRepo;
        this.assignmentRepo = assignmentRepo;
        this.gradeRepo = gradeRepo;
        this.currentUser = currentUser;
    }

    public async Task<IReadOnlyList<MySubmissionDto>> Handle(GetMySubmissionsQuery request, CancellationToken ct)
    {
        var studentId = currentUser.UserId ?? throw new InvalidOperationException("User must be authenticated.");

        var submissions = await submissionRepo.GetListAsync(s => s.StudentId == studentId);

        var assignmentIds = submissions.Select(s => s.AssignmentId).Distinct().ToArray();
        var assignments = await assignmentRepo.GetListAsync(a => assignmentIds.Contains(a.Id));
        var assignmentMap = assignments.ToDictionary(a => a.Id, a => a);

        var subIds = submissions.Select(s => s.Id).ToArray();
        var grades = await gradeRepo.GetListAsync(g => subIds.Contains(g.SubmissionId));
        var gradeMap = grades.ToDictionary(g => g.SubmissionId, g => g);

        return submissions.Select(s => new MySubmissionDto
        {
            SubmissionId = s.Id,
            AssignmentId = s.AssignmentId,
            AssignmentTitle = assignmentMap.TryGetValue(s.AssignmentId, out var a) ? a.Title : "",
            SubmittedAt = s.SubmittedAt,
            Status = s.Status.ToString(),
            Score = gradeMap.TryGetValue(s.Id, out var g) ? g.Score : null
        }).ToList();
    }


}
