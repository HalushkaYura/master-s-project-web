using SmartClass.Application.Contracts.Auth;

namespace SmartClass.Application.Abstractions
{
    public interface IJwtTokenService
    {
        Task<TokenPairDto> CreateTokensAsync(
            Guid userId,
            string email,
            string? displayName,
            IReadOnlyCollection<string> roles,
            CancellationToken ct = default);

        Task<TokenPairDto> RefreshTokensAsync(string userId, string refreshToken, CancellationToken ct);

        Guid? GetUserIdFromAccessToken(string token);
    }
}
