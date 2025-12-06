using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SmartClass.Application.Abstractions;
using SmartClass.Application.Contracts.Auth;
using SmartClass.Infrastructure.Identity.Entities;
using SmartClass.Infrastructure.Options;
using SmartClass.Infrastructure.Persistence;

namespace SmartClass.Infrastructure.Services.Auth
{
    public class JwtTokenService : IJwtTokenService
    {
        private readonly JwtOptions jwtOptions;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly AppDbContext db;

        public JwtTokenService(
            IOptions<JwtOptions> jwtOptions,
            UserManager<ApplicationUser> userManager,
            AppDbContext db)
        {
            this.jwtOptions = jwtOptions.Value;
            this.userManager = userManager;
            this.db = db;
        }

        public async Task<TokenPairDto> CreateTokensAsync(
            Guid userId,
            string email,
            string? displayName,
            IReadOnlyCollection<string> roles,
            CancellationToken ct = default)
        {
            var now = DateTime.UtcNow;
            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, userId.ToString()),
                new(JwtRegisteredClaimNames.Email, email),
                new(ClaimTypes.Name, displayName ?? email),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new(JwtRegisteredClaimNames.Nbf, new DateTimeOffset(now).ToUnixTimeSeconds().ToString()),
                new(JwtRegisteredClaimNames.Iat, new DateTimeOffset(now).ToUnixTimeSeconds().ToString())
            };

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Key));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var accessTokenExpires = now.AddMinutes(jwtOptions.AccessTokenMinutes);

            var token = new JwtSecurityToken(
                issuer: jwtOptions.Issuer,
                audience: jwtOptions.Audience,
                claims: claims,
                notBefore: now,
                expires: accessTokenExpires,
                signingCredentials: creds
            );

            var accessToken = new JwtSecurityTokenHandler().WriteToken(token);

            // Refresh token
            var refreshToken = Convert.ToBase64String(Guid.NewGuid().ToByteArray()) +
                               Convert.ToBase64String(Guid.NewGuid().ToByteArray());
            var refreshExpires = now.AddDays(jwtOptions.RefreshTokenDays);

            var entity = new RefreshToken
            {
                UserId = userId,
                Token = refreshToken,
                ExpiresAt = refreshExpires,
                IsRevoked = false
            };

            db.RefreshTokens.Add(entity);
            await db.SaveChangesAsync(ct);

            return new TokenPairDto(
                AccessToken: accessToken,
                RefreshToken: new RefreshTokenDto(refreshToken, refreshExpires)
            );
        }

        public async Task<TokenPairDto> RefreshTokensAsync(string userIdRaw, string refreshToken, CancellationToken ct)
        {
            if (!Guid.TryParse(userIdRaw, out var userId))
                throw new InvalidOperationException("Invalid user id.");

            var tokenEntity = await db.RefreshTokens
                .AsNoTracking()
                .FirstOrDefaultAsync(x =>
                    x.UserId == userId &&
                    x.Token == refreshToken &&
                    !x.IsRevoked &&
                    x.ExpiresAt > DateTime.UtcNow,
                    ct);

            if (tokenEntity is null)
                throw new InvalidOperationException("Invalid or expired refresh token.");

            var user = await userManager.FindByIdAsync(userId.ToString())
                       ?? throw new InvalidOperationException("User not found.");

            var roles = await userManager.GetRolesAsync(user);

            // Помітити старий refresh як Revoke (опціонально)
            var existing = await db.RefreshTokens.FirstAsync(x => x.Id == tokenEntity.Id, ct);
            existing.IsRevoked = true;
            await db.SaveChangesAsync(ct);

            return await CreateTokensAsync(user.Id, user.Email!, user.DisplayName ?? user.UserName, roles.ToArray(), ct);
        }

        public Guid? GetUserIdFromAccessToken(string token)
        {
            var handler = new JwtSecurityTokenHandler();
            if (!handler.CanReadToken(token))
                return null;

            var jwt = handler.ReadJwtToken(token);
            var sub = jwt.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub)?.Value;
            if (Guid.TryParse(sub, out var id))
                return id;
            return null;
        }
    }
}
