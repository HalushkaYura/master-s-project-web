using MediatR;
using SmartClass.Application.Abstractions;
using SmartClass.Application.Abstractions.Storage;
using SmartClass.Domain.Entities;

namespace SmartClass.Application.Features.Files.Commands.UploadSubmissionFiles;

public sealed class UploadSubmissionFilesHandler
    : IRequestHandler<UploadSubmissionFilesCommand, IReadOnlyList<Guid>>
{
    private readonly IRepository<Submission> submissionRepo;
    private readonly IRepository<FileResource> fileRepo;
    private readonly ICurrentUserService currentUser;
    private readonly IFileStorage storage;

    public UploadSubmissionFilesHandler(
        IRepository<Submission> submissionRepo,
        IRepository<FileResource> fileRepo,
        ICurrentUserService currentUser,
        IFileStorage storage)
    {
        this.submissionRepo = submissionRepo;
        this.fileRepo = fileRepo;
        this.currentUser = currentUser;
        this.storage = storage;
    }

    public async Task<IReadOnlyList<Guid>> Handle(UploadSubmissionFilesCommand request, CancellationToken ct)
    {
        var userId = currentUser.UserId ?? throw new InvalidOperationException("User must be authenticated.");

        var submission = await submissionRepo.GetByKeyAsync(request.SubmissionId)
            ?? throw new InvalidOperationException("Submission not found.");

        if (submission.StudentId != userId)
            throw new InvalidOperationException("You can upload files only to your own submission.");

        var savedIds = new List<Guid>();

        foreach (var f in request.Files)
        {
            // /uploads/submissions/{submissionId}/{filename}
            var relPath = Path.Combine("uploads", "submissions", submission.Id.ToString("N"), f.FileName);
            var blobPath = await storage.SaveAsync(f.Content, relPath, ct);

            var fr = new FileResource
            {
                Id = Guid.NewGuid(),
                OwnerId = userId,
                SubmissionId = submission.Id,
                FileName = f.FileName,
                ContentType = f.ContentType,
                SizeBytes = f.SizeBytes,
                BlobPath = "/" + blobPath.Replace('\\', '/'),
                UploadedAt = DateTime.UtcNow
            };

            await fileRepo.AddAsync(fr);
            savedIds.Add(fr.Id);
        }

        await fileRepo.SaveChangesAsync();
        return savedIds;
    }
}
