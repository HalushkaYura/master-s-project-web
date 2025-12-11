namespace SmartClass.Application.Contracts.Submissions
{
    public sealed record EnsureSubmissionDto(
        Guid AssignmentId,
        Guid StudentId
    );
}
