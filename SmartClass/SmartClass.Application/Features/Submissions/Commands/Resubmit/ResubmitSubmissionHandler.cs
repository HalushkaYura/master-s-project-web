using MediatR;
using SmartClass.Application.Abstractions;
using SmartClass.Application.Abstractions.Storage;
using SmartClass.Domain.Entities;
using SmartClass.Domain.Enums;

namespace SmartClass.Application.Features.Submissions.Commands.Resubmit;

public sealed class ResubmitSubmissionHandler : IRequestHandler<ResubmitSubmissionCommand, Guid>
{
    private readonly IRepository<Submission> submissionRepo;
    private readonly IRepository<Assignment> assignmentRepo;
    private readonly IRepository<FileResource> fileRepo;
    private readonly IFileStorage storage;
    private readonly ICurrentUserService currentUser;

    public ResubmitSubmissionHandler(
        IRepository<Submission> submissionRepo,
        IRepository<Assignment> assignmentRepo,
        IRepository<FileResource> fileRepo,
        IFileStorage storage,
        ICurrentUserService currentUser)
    {
        this.submissionRepo = submissionRepo;
        this.assignmentRepo = assignmentRepo;
        this.fileRepo = fileRepo;
        this.storage = storage;
        this.currentUser = currentUser;
    }

    public async Task<Guid> Handle(ResubmitSubmissionCommand request, CancellationToken ct)
    {
        var studentId = currentUser.UserId ?? throw new InvalidOperationException("User must be authenticated.");

        var assignment = await assignmentRepo.GetByKeyAsync(request.AssignmentId)
            ?? throw new InvalidOperationException("Assignment not found.");

        if (assignment.Status == "Closed")
            throw new InvalidOperationException("Assignment is closed.");
        if (assignment.DueAt.HasValue && assignment.DueAt.Value < DateTime.UtcNow && !assignment.AllowLate)
            throw new InvalidOperationException("Deadline exceeded.");

        // шукаємо існуючий submission цього студента
        var existing = (await submissionRepo.GetListAsync(s => s.AssignmentId == request.AssignmentId && s.StudentId == studentId)).FirstOrDefault();
        if (existing is null)
            throw new InvalidOperationException("No previous submission found. Submit first.");

        // видаляємо старі файли з диска та БД
        var oldFiles = await fileRepo.GetListAsync(f => f.SubmissionId == existing.Id);
        foreach (var f in oldFiles)
        {
            try { await storage.DeleteAsync(f.BlobPath, ct); } catch { /* ignore */ }
            await fileRepo.DeleteAsync(f);
        }

        // оновлюємо submission timestamp і статус
        existing.SubmittedAt = DateTime.UtcNow;
        existing.Status = SubmissionStatus.Submitted;
        await submissionRepo.UpdateAsync(existing);

        // зберігаємо нові файли
        foreach (var file in request.Files)
        {
            var relPath = Path.Combine("uploads", "submissions", existing.Id.ToString("N"), file.FileName);
            var blobPath = await storage.SaveAsync(file.Content, relPath, ct);

            var fr = new FileResource
            {
                Id = Guid.NewGuid(),
                OwnerId = studentId,
                SubmissionId = existing.Id,
                FileName = file.FileName,
                ContentType = file.ContentType,
                SizeBytes = file.SizeBytes,
                BlobPath = "/" + blobPath.Replace('\\', '/'),
                UploadedAt = DateTime.UtcNow
            };
            await fileRepo.AddAsync(fr);
        }

        await submissionRepo.SaveChangesAsync();
        await fileRepo.SaveChangesAsync();

        return existing.Id;
    }
}
