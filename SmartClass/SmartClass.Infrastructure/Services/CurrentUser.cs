using Microsoft.AspNetCore.Http;
using SmartClass.Application.Abstractions;
using System.Security.Claims;

namespace SmartClass.Infrastructure.Services;

public class CurrentUser : ICurrentUser
{
    private readonly IHttpContextAccessor httpContextAccessor;
    public CurrentUser(IHttpContextAccessor httpContextAccessor) => this.httpContextAccessor = httpContextAccessor;

    private ClaimsPrincipal? Principal => httpContextAccessor.HttpContext?.User;

    public bool IsAuthenticated => Principal?.Identity?.IsAuthenticated ?? false;

    public Guid? UserId
    {
        get
        {
            var sub = Principal?.FindFirstValue("sub")
                      ?? Principal?.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(sub, out var g) ? g : null;
        }
    }

    public string? Email => Principal?.FindFirstValue(ClaimTypes.Email);
    public string? Name => Principal?.Identity?.Name;

    public IReadOnlyCollection<string> Roles =>
        Principal?.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value).ToArray()
        ?? Array.Empty<string>();
}
