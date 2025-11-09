using SmartClass.Application.Contracts.Auth;

namespace SmartClass.Application.Abstractions;
public interface IJwtTokenService
{
    // Створити access + refresh без знання про Identity-модель
    Task<TokenPairDto> CreateTokensAsync(
            Guid userId,
            string? email,
            string? displayName,
            IEnumerable<string> roles,
            CancellationToken cancellationToken);

    // Повертає новий refresh і access, якщо поточний дійсний
     Task<TokenPairDto?> RotateRefreshTokenAsync(
         Guid userId,
         string currentRefreshToken,
         CancellationToken ct);
}

