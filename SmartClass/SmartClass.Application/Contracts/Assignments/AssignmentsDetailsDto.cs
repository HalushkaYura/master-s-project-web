namespace SmartClass.Application.Contracts.Assignments
{
    public sealed record AssignmentDetailsDto(
        Guid Id,
        Guid ClassroomId,
        Guid CreatedBy,
        string Title,
        string? DescriptionHtml,
        int PointsMax,
        DateTime? DueAt,
        bool AllowLate,
        string Status,
        Guid? MaterialId,
        string? MaterialTitle,
        IReadOnlyList<FileResourceDto> Files
    // пізніше тут можна додати: список submissions і оцінок
    );
}
