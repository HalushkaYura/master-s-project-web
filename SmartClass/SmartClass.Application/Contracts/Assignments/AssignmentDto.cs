namespace SmartClass.Application.Contracts.Assignments;


public record AssignmentDto(
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
