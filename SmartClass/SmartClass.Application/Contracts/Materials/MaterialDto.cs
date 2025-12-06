namespace SmartClass.Application.Contracts.Classrooms
{
    public record MaterialDto(
        Guid Id,
        Guid ClassroomId,
        Guid CreatedBy,
        string Title,
        string? Description,
        string? ContentHtml,
        DateTime CreatedAt,
        DateTime? UpdatedAt
    );
}