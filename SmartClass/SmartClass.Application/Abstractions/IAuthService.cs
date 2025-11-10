using SmartClass.Application.Contracts.Auth;

namespace SmartClass.Application.Abstractions;

public interface IAuthService
{
    Task<TokenPairDto> RegisterAsync(RegisterDto request, CancellationToken cancellationToken);
    Task<TokenPairDto> LoginAsync(LoginDto request, CancellationToken cancellationToken);
    Task LogoutAsync(LogoutDto request, CancellationToken cancellationToken);
}
