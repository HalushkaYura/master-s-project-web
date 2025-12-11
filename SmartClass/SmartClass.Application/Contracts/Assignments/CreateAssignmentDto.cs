using System;

namespace SmartClass.Application.Contracts.Assignments
{
    public record CreateAssignmentDto(
        Guid ClassroomId,
        string Title,
        string? DescriptionHtml,
        int PointsMax,
        DateTime? DueAt,
        bool AllowLate,
        Guid? MaterialId // ← додаємо це
    );
}
