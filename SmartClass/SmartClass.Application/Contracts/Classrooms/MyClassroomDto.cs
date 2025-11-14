namespace SmartClass.Application.Contracts.Classrooms;

public record MyClassroomDto(
    Guid Id,
    string Title,
    string? Section,
    string RoleInClass,
    string JoinCode
);
