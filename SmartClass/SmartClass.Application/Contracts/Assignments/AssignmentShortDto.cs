namespace SmartClass.Application.Contracts.Assignments
{
    public record AssignmentShortDto(
    Guid Id,
    Guid ClassroomId,
    Guid CreatedBy,
    string Title,
    string? DescriptionHtml,
    int PointsMax,
    DateTime? DueAt,
    bool AllowLate,
    string Status
);

}


