namespace SmartClass.Application.Contracts.Classrooms;

public sealed class ClassroomDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Section { get; set; }
    public string JoinCode { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsArchived { get; set; }
    public string RoleInClass { get; set; } = string.Empty;
}
