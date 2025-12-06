using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SmartClass.Application.Abstractions;
using SmartClass.Application.Contracts.Auth;
using SmartClass.Infrastructure.Options;

namespace SmartClass.Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService authService;
        private readonly JwtOptions jwtOptions;

        public AuthController(
            IAuthService authService,
            IOptions<JwtOptions> jwtOptions)
        {
            this.authService = authService;
            this.jwtOptions = jwtOptions.Value;
        }

        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] RegisterDto request, CancellationToken ct)
        {
            var tokens = await authService.RegisterAsync(request, ct);

            SetAuthCookies(tokens);

            // Якщо на фронті токени більше не потрібні – достатньо Ok()
            return Ok(tokens);
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginDto request, CancellationToken ct)
        {
            try
            {
                var tokens = await authService.LoginAsync(request, ct);

                SetAuthCookies(tokens);

                return Ok(tokens); // або Ok() – якщо клієнт не використовує токени напряму
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("refresh")]
        [AllowAnonymous]
        public async Task<IActionResult> Refresh([FromBody] RefreshDto request, CancellationToken ct)
        {
            var tokens = await authService.RefreshAsync(request, ct);

            SetAuthCookies(tokens);

            return Ok(tokens);
        }

        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout([FromBody] LogoutDto request, CancellationToken ct)
        {
            await authService.LogoutAsync(request, ct);

            ClearAuthCookies();

            return NoContent();
        }

        // ---------- ДОПОМІЖНІ МЕТОДИ ДЛЯ КУКІ ----------

        private void SetAuthCookies(TokenPairDto tokens)
        {
            // accessToken cookie
            Response.Cookies.Append(
                "accessToken",
                tokens.AccessToken,
                new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    // Використовуємо JwtOptions, а не "60"
                    Expires = DateTimeOffset.UtcNow.AddMinutes(jwtOptions.AccessTokenMinutes)
                });

            // refreshToken cookie
            Response.Cookies.Append(
                "refreshToken",
                tokens.RefreshToken.Token,
                new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    // Тут логічно ставити те саме, що в самому токені
                    Expires = tokens.RefreshToken.ExpiresAt
                });
        }

        private void ClearAuthCookies()
        {
            Response.Cookies.Delete("accessToken");
            Response.Cookies.Delete("refreshToken");
        }

        // ---------- ДОДАТКОВІ СЕРВІСНІ ЕНДПОІНТИ ----------

        [HttpGet("IsAuthenticated")]
        [Authorize]
        public IActionResult IsAuthenticated()
            => Ok(User.Identity?.IsAuthenticated ?? false);

        [HttpGet("UserIdAuthenticated")]
        [Authorize]
        public IActionResult UserIdAuthenticated()
        {
            var userId =
                User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ??
                User.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User ID claim not found.");
            }

            return Ok(userId);
        }
    }
}
