namespace SmartClass.Application.Contracts.Classrooms;

public record CreateClassroomDto(
    string Title,
    string? Section,
    string? Description
);
