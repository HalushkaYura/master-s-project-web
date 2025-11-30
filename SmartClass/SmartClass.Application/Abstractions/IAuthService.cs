using SmartClass.Application.Contracts.Auth;

namespace SmartClass.Application.Abstractions
{
    public interface IAuthService
    {
        Task<TokenPairDto> RegisterAsync(RegisterDto request, CancellationToken ct);
        Task<TokenPairDto> LoginAsync(LoginDto request, CancellationToken ct);
        Task LogoutAsync(LogoutDto request, CancellationToken ct);
        Task<TokenPairDto> RefreshAsync(RefreshDto request, CancellationToken ct);
    }
}
