using SmartClass.Domain.Enums;

namespace SmartClass.Application.Contracts.Submissions
{
    public sealed record SubmissionListItemDto(
        Guid Id,
        Guid StudentId,
        string StudentName,
        SubmissionStatus Status,
        DateTime? SubmittedAt,
        decimal? Score
    );
}

