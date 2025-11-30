using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartClass.Application.Abstractions;
using SmartClass.Application.Contracts.Auth;

namespace SmartClass.Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService authService;

        public AuthController(IAuthService authService)
        {
            this.authService = authService;
        }

        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] RegisterDto request, CancellationToken ct)
        {
            var result = await authService.RegisterAsync(request, ct);
            return Ok(result);
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginDto request, CancellationToken ct)
        {
            try
            {
                var result = await authService.LoginAsync(request, ct);
                return Ok(result);
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
            var result = await authService.RefreshAsync(request, ct);
            return Ok(result);
        }

        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout([FromBody] LogoutDto request, CancellationToken ct)
        {
            await authService.LogoutAsync(request, ct);
            return NoContent();
        }






        [HttpGet]
        [Authorize]
        [Route("IsAuthenticated")]
        public IActionResult IsAuthenticated() => Ok(User.Identity.IsAuthenticated);

        [HttpGet]
        [Route("UserIdAuthenticated")]
        [Authorize] 
        public IActionResult UserIdAuthenticated()
        {
            // Отримуємо ID користувача (ClaimsPrincipal.Identity)
            // ID користувача зазвичай зберігається у клеймі (claim) NameIdentifier.
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                // Хоча користувач авторизований, ID може бути відсутній
                return Unauthorized("User ID claim not found.");
            }

            return Ok(userId);
        }
    }
}
