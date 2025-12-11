namespace SmartClass.Application.Contracts.Classrooms
{
    public sealed record ClassroomDetailsDto(
        Guid Id,
        string Title,
        string? Section,
        string? Description,
        string JoinCode,
        bool IsArchived,
        string OwnerName,
        IReadOnlyList<ClassroomMemberDto> Members
    );
}
