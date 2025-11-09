using Microsoft.AspNetCore.Identity;
using SmartClass.Application.Abstractions;
using SmartClass.Application.Contracts.Auth;
using SmartClass.Infrastructure.Identity.Entities;

namespace SmartClass.Web;

public static class AuthEndpoints
{
    public static async Task<IResult> Register(
        UserManager<ApplicationUser> um,
        RoleManager<ApplicationRole> rm,
        IJwtTokenService tokens,
        RegisterDto dto,
        CancellationToken ct)
    {
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = dto.Email,
            Email = dto.Email,
            DisplayName = dto.DisplayName
        };

        var res = await um.CreateAsync(user, dto.Password);
        if (!res.Succeeded) return Results.BadRequest(res.Errors);

        foreach (var r in new[] { "Teacher", "Student" })
            if (!await rm.RoleExistsAsync(r)) await rm.CreateAsync(new ApplicationRole { Name = r });

        if (!string.IsNullOrWhiteSpace(dto.Role))
            await um.AddToRoleAsync(user, dto.Role);

        var roles = await um.GetRolesAsync(user);
        var pair = await tokens.CreateTokensAsync(user.Id, user.Email, user.DisplayName ?? user.UserName, roles, ct);
        return Results.Ok(pair);
    }

    public static async Task<IResult> Login(
        SignInManager<ApplicationUser> sm,
        UserManager<ApplicationUser> um,
        IJwtTokenService tokens,
        LoginDto dto,
        CancellationToken ct)
    {
        var user = await um.FindByEmailAsync(dto.Email);
        if (user is null) return Results.Unauthorized();

        var res = await sm.CheckPasswordSignInAsync(user, dto.Password, false);
        if (!res.Succeeded) return Results.Unauthorized();

        var roles = await um.GetRolesAsync(user);
        var pair = await tokens.CreateTokensAsync(user.Id, user.Email, user.DisplayName ?? user.UserName, roles, ct);
        return Results.Ok(pair);
    }

    public static async Task<IResult> Refresh(
        UserManager<ApplicationUser> um,
        IJwtTokenService tokens,
        RefreshDto dto,
        CancellationToken ct)
    {
        if (!Guid.TryParse(dto.UserId, out var uid)) return Results.BadRequest();
        var user = await um.FindByIdAsync(uid.ToString());
        if (user is null) return Results.Unauthorized();

        var pair = await tokens.RotateRefreshTokenAsync(uid, dto.RefreshToken, ct);
        return pair is null ? Results.Unauthorized() : Results.Ok(pair);
    }
}
