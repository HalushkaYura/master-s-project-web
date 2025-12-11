using SmartClass.Domain.Enums;
using SmartClass.Application.Contracts;

namespace SmartClass.Application.Contracts.Submissions
{
    public sealed record SubmissionDetailsDto(
        Guid Id,
        Guid AssignmentId,
        Guid StudentId,
        string StudentName,
        SubmissionStatus Status,
        DateTime? SubmittedAt,
        decimal? Score,
        string? TeacherComment,
        IReadOnlyList<FileResourceDto> Files
    );
}
