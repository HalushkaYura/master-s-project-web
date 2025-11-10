using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace SmartClass.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AccountController : ControllerBase
{
    [Authorize] // доступ тільки з валідним JWT
    [HttpGet("me")]
    public IActionResult Me()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                     ?? User.FindFirstValue(ClaimTypes.Name) // fallback
                     ?? User.FindFirstValue("sub");          // якщо так налаштовано у токені

        var email = User.FindFirstValue(ClaimTypes.Email);
        var name = User.Identity?.Name; // ми писали його в ClaimTypes.Name
        var roles = User.Claims
                        .Where(c => c.Type == ClaimTypes.Role)
                        .Select(c => c.Value)
                        .ToArray();

        return Ok(new
        {
            userId,
            email,
            name,
            roles
        });
    }

    // Додатковий чек ролей (за бажанням)
    [Authorize(Roles = "Teacher")]
    [HttpGet("teacher-only")]
    public IActionResult OnlyForTeachers()
    {
        return Ok(new { ok = true, message = "You are a Teacher" });
    }
}
