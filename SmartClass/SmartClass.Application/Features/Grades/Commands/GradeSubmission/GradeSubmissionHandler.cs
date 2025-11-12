using MediatR;
using SmartClass.Application.Abstractions;
using SmartClass.Domain.Entities;

namespace SmartClass.Application.Features.Grades.Commands.GradeSubmission;

public sealed class GradeSubmissionHandler : IRequestHandler<GradeSubmissionCommand, Guid>
{
    private readonly IRepository<Submission> submissionRepo;
    private readonly IRepository<Assignment> assignmentRepo;
    private readonly IRepository<Classroom> classroomRepo;
    private readonly IRepository<Grade> gradeRepo;
    private readonly ICurrentUser currentUser;

    public GradeSubmissionHandler(
        IRepository<Submission> submissionRepo,
        IRepository<Assignment> assignmentRepo,
        IRepository<Classroom> classroomRepo,
        IRepository<Grade> gradeRepo,
        ICurrentUser currentUser)
    {
        this.submissionRepo = submissionRepo;
        this.assignmentRepo = assignmentRepo;
        this.classroomRepo = classroomRepo;
        this.gradeRepo = gradeRepo;
        this.currentUser = currentUser;
    }

    public async Task<Guid> Handle(GradeSubmissionCommand request, CancellationToken ct)
    {
        var teacherId = currentUser.UserId
            ?? throw new InvalidOperationException("User must be authenticated.");

        var submission = await submissionRepo.GetByKeyAsync(request.SubmissionId)
            ?? throw new InvalidOperationException("Submission not found.");

        var assignment = await assignmentRepo.GetByKeyAsync(submission.AssignmentId)
            ?? throw new InvalidOperationException("Assignment not found.");

        var classroom = await classroomRepo.GetByKeyAsync(assignment.ClassroomId)
            ?? throw new InvalidOperationException("Classroom not found.");

        // Перевірка прав: лише власник класу (Teacher) може оцінювати
        if (classroom.OwnerId != teacherId)
            throw new InvalidOperationException("Only the class owner can grade submissions.");

        if (request.Score < 0 || request.Score > assignment.PointsMax)
            throw new InvalidOperationException($"Score must be between 0 and {assignment.PointsMax}.");

        // Одна оцінка на submission: upsert
        var existing = await gradeRepo.GetListAsync(g => g.SubmissionId == submission.Id);
        Grade grade;
        if (existing.Any())
        {
            grade = existing.First();
            grade.Score = request.Score;
            grade.FeedbackHtml = request.FeedbackHtml;
            grade.GradedAt = DateTime.UtcNow;
            await gradeRepo.UpdateAsync(grade);
        }
        else
        {
            grade = new Grade
            {
                Id = Guid.NewGuid(),
                SubmissionId = submission.Id,
                GradedBy = teacherId,
                Score = request.Score,
                FeedbackHtml = request.FeedbackHtml,
                GradedAt = DateTime.UtcNow
            };
            await gradeRepo.AddAsync(grade);
        }

        await gradeRepo.SaveChangesAsync();
        return grade.Id;
    }
}
