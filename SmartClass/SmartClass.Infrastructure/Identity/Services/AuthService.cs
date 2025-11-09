using Microsoft.AspNetCore.Identity;
using SmartClass.Application.Abstractions;
using SmartClass.Application.Contracts.Auth;
using SmartClass.Infrastructure.Identity.Entities;
using SmartClass.Infrastructure.Persistence;

namespace SmartClass.Infrastructure.Identity.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> userManager;
    private readonly RoleManager<ApplicationRole> roleManager;
    private readonly SignInManager<ApplicationUser> signInManager;
    private readonly IJwtTokenService jwtTokenService;
    private readonly AppDbContext dbContext;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager,
        SignInManager<ApplicationUser> signInManager,
        IJwtTokenService jwtTokenService,
        AppDbContext dbContext)
    {
        this.userManager = userManager;
        this.roleManager = roleManager;
        this.signInManager = signInManager;
        this.jwtTokenService = jwtTokenService;
        this.dbContext = dbContext;
    }

    public async Task<TokenPairDto> RegisterAsync(RegisterDto request, CancellationToken cancellationToken)
    {
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            Email = request.Email,
            UserName = request.Email,
            DisplayName = request.DisplayName
        };

        var createResult = await userManager.CreateAsync(user, request.Password);
        if (!createResult.Succeeded)
            throw new InvalidOperationException(string.Join("; ", createResult.Errors.Select(e => e.Description)));

        if (!string.IsNullOrWhiteSpace(request.Role))
        {
            if (!await roleManager.RoleExistsAsync(request.Role))
                await roleManager.CreateAsync(new ApplicationRole { Name = request.Role });
            await userManager.AddToRoleAsync(user, request.Role);
        }

        var roles = await userManager.GetRolesAsync(user);
        return await jwtTokenService.CreateTokensAsync(user.Id, user.Email, user.DisplayName ?? user.UserName, roles, cancellationToken);
    }

    public async Task<TokenPairDto> LoginAsync(LoginDto request, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByEmailAsync(request.Email)
                   ?? throw new InvalidOperationException("Invalid credentials.");

        var signInResult = await signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: false);
        if (!signInResult.Succeeded)
            throw new InvalidOperationException("Invalid credentials.");

        var roles = await userManager.GetRolesAsync(user);
        return await jwtTokenService.CreateTokensAsync(user.Id, user.Email, user.DisplayName ?? user.UserName, roles, cancellationToken);
    }

    public async Task LogoutAsync(LogoutDto request, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(request.UserId, out var userId))
            throw new InvalidOperationException("Invalid user id.");

        var token = dbContext.RefreshTokens.FirstOrDefault(x =>
            x.UserId == userId && x.Token == request.RefreshToken && !x.IsRevoked);

        if (token is null) return;

        token.IsRevoked = true;
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
