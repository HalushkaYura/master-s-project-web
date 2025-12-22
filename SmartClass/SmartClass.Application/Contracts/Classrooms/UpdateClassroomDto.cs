public sealed record UpdateClassroomDto(
    Guid Id,
    string Title,
    string? Section,
    string? Description
);