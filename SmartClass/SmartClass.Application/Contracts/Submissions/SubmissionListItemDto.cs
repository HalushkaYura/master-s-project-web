using SmartClass.Domain.Enums;

namespace SmartClass.Application.Contracts.Submissions
{
    public sealed record SubmissionListItemDto(
        Guid Id,
        Guid StudentId,
        string StudentName,
        DateTime? SubmittedAt,
        SubmissionStatus Status,
        decimal? Score
    );
}
