using System.Security.Claims;
using Microsoft.AspNetCore.SignalR;

namespace SmartClass.Web.Auth;

public sealed class GuidUserIdProvider : IUserIdProvider
{
    public string? GetUserId(HubConnectionContext connection)
        => connection.User?.FindFirstValue(ClaimTypes.NameIdentifier);
}
