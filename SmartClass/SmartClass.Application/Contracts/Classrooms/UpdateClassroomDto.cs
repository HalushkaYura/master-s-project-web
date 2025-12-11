public record UpdateClassroomDto(
    string Title,
    string? Section,
    string? Description,
    bool IsArchived
);