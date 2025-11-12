using MediatR;

namespace SmartClass.Application.Features.Files.Commands.UploadSubmissionFiles;

public sealed class UploadSubmissionFilesCommand : IRequest<IReadOnlyList<Guid>>
{
    public Guid SubmissionId { get; init; }
    public IReadOnlyList<UploadFileItem> Files { get; init; } = Array.Empty<UploadFileItem>();
}

public sealed class UploadFileItem
{
    public string FileName { get; init; } = string.Empty;
    public string ContentType { get; init; } = "application/octet-stream";
    public Stream Content { get; init; } = Stream.Null;
    public long SizeBytes { get; init; }
}
