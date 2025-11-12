namespace SmartClass.Application.Contracts.Assignments;

public sealed class AssignmentDto
{
    public Guid Id { get; set; }
    public Guid ClassroomId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? DescriptionHtml { get; set; }
    public int PointsMax { get; set; }
    public DateTime? DueAt { get; set; }
    public bool AllowLate { get; set; }
    public string Status { get; set; } = string.Empty;
}
