namespace SmartClass.Application.Contracts.Submissions;

public sealed class MySubmissionDto
{
    public Guid SubmissionId { get; set; }
    public Guid AssignmentId { get; set; }
    public string AssignmentTitle { get; set; } = string.Empty;
    public DateTime? SubmittedAt { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal? Score { get; set; }
}
