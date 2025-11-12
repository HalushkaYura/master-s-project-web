using MediatR;

namespace SmartClass.Application.Features.Submissions.Commands.Resubmit;

public sealed class ResubmitSubmissionCommand : IRequest<Guid>
{
    public Guid AssignmentId { get; init; }
    public IReadOnlyList<ResubmitFileItem> Files { get; init; } = Array.Empty<ResubmitFileItem>();
}

public sealed class ResubmitFileItem
{
    public string FileName { get; init; } = string.Empty;
    public string ContentType { get; init; } = "application/octet-stream";
    public Stream Content { get; init; } = Stream.Null;
    public long SizeBytes { get; init; }
}
