namespace SmartClass.Application.Contracts.Submissions
{
    public sealed record GradeSubmissionDto(
        Guid SubmissionId,
        decimal Score,
        string? Comment
    );
}
