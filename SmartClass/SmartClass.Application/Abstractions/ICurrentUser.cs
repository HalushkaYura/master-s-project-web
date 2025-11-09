namespace SmartClass.Application.Abstractions;
public interface ICurrentUser
{
    bool IsAuthenticated { get; }
    Guid? UserId { get; }
    string? Email { get; }
    string? Name { get; }
    IReadOnlyCollection<string> Roles { get; }
}
