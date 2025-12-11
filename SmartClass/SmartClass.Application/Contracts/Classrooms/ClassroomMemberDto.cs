namespace SmartClass.Application.Contracts.Classrooms
{
    public sealed record ClassroomMemberDto(
        Guid UserId,
        string FullName,
        string Email,
        string RoleInClass
    );
}