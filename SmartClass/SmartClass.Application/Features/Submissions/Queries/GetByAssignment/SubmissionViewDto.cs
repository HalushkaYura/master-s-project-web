namespace SmartClass.Application.Features.Submissions.Queries.GetByAssignment;

public sealed class SubmissionViewDto
{
    public Guid SubmissionId { get; set; }
    public Guid StudentId { get; set; }
    public DateTime? SubmittedAt { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal? Score { get; set; }
    public string? FeedbackHtml { get; set; }
}
