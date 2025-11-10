using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SmartClass.Application.Abstractions;
using SmartClass.Application.Contracts.Auth;
using SmartClass.Infrastructure.Identity.Entities;
using SmartClass.Infrastructure.Options;
using SmartClass.Infrastructure.Persistence;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SmartClass.Infrastructure.Identity.Services;

public class JwtTokenService : IJwtTokenService
{
    private readonly UserManager<ApplicationUser> userManager;
    private readonly AppDbContext dbContext;
    private readonly JwtOptions options;

    public JwtTokenService(
        UserManager<ApplicationUser> userManager,
        AppDbContext dbContext,
        IOptions<JwtOptions> optionsAccessor)
    {
        this.userManager = userManager;
        this.dbContext = dbContext;
        options = optionsAccessor.Value;
    }

    public async Task<TokenPairDto> CreateTokensAsync(
        Guid userId, string? email, string? displayName, IEnumerable<string> roles, CancellationToken cancellationToken)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new(JwtRegisteredClaimNames.Email, email ?? ""),
            new(ClaimTypes.Name, displayName ?? email ?? "")
        };
        claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(options.Key));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var jwt = new JwtSecurityToken(
            issuer: options.Issuer,
            audience: options.Audience,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: DateTime.UtcNow.AddMinutes(options.AccessTokenMinutes),
            signingCredentials: credentials);

        var accessToken = new JwtSecurityTokenHandler().WriteToken(jwt);

        var refresh = new RefreshToken
        {
            UserId = userId,
            Token = Convert.ToBase64String(Guid.NewGuid().ToByteArray()),
            ExpiresAt = DateTime.UtcNow.AddDays(options.RefreshTokenDays),
            IsRevoked = false
        };
        dbContext.RefreshTokens.Add(refresh);
        await dbContext.SaveChangesAsync(cancellationToken);

        return new TokenPairDto(accessToken, new RefreshTokenDto(refresh.Token, refresh.ExpiresAt));
    }

    public async Task<TokenPairDto?> RotateRefreshTokenAsync(
    Guid userId,
    string currentRefreshToken,
    CancellationToken cancellationToken)
    {
        // шукаємо refresh у БД
        var oldToken = await dbContext.RefreshTokens
            .FirstOrDefaultAsync(x => x.UserId == userId && x.Token == currentRefreshToken && !x.IsRevoked, cancellationToken);

        if (oldToken is null || oldToken.ExpiresAt < DateTime.UtcNow)
            return null; // токен не знайдено або протермінований

        // відмічаємо старий токен як відкликаний
        oldToken.IsRevoked = true;
        oldToken.ReplacedByToken = Guid.NewGuid().ToString();

        // створюємо нову пару токенів
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user is null) return null;

        var roles = await userManager.GetRolesAsync(user);
        var newPair = await CreateTokensAsync(user.Id, user.Email, user.DisplayName ?? user.UserName, roles, cancellationToken);

        await dbContext.SaveChangesAsync(cancellationToken);

        return newPair;
    }


}
