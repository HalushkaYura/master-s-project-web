using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SmartClass.Application.Abstractions;
using SmartClass.Application.Contracts.Auth;
using SmartClass.Infrastructure.Identity.Entities;
using SmartClass.Infrastructure.Persistence;

namespace SmartClass.Infrastructure.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly SignInManager<ApplicationUser> signInManager;
        private readonly RoleManager<ApplicationRole> roleManager;
        private readonly IJwtTokenService jwtService;
        private readonly AppDbContext db;

        public AuthService(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            RoleManager<ApplicationRole> roleManager,
            IJwtTokenService jwtService,
            AppDbContext db)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.roleManager = roleManager;
            this.jwtService = jwtService;
            this.db = db;
        }

        public async Task<TokenPairDto> RegisterAsync(RegisterDto request, CancellationToken ct)
        {
            // Перевірка, чи немає вже користувача
            var existing = await userManager.FindByEmailAsync(request.Email);
            if (existing != null)
                throw new InvalidOperationException("User with this email already exists.");

            var user = new ApplicationUser
            {
                Id = Guid.NewGuid(),
                Email = request.Email,
                UserName = request.Email,
                DisplayName = $"{request.FirstName} {request.LastName}".Trim(),
                Firstname = request.FirstName,
                Lastname = request.LastName,
                BirthDate = request.BirthDate
            };

            var createResult = await userManager.CreateAsync(user, request.Password);
            if (!createResult.Succeeded)
            {
                var errors = string.Join("; ", createResult.Errors.Select(e => e.Description));
                throw new InvalidOperationException(errors);
            }

            // Роль
            var role = string.IsNullOrWhiteSpace(request.Role) ? "Student" : request.Role;
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new ApplicationRole { Name = role });
            }

            await userManager.AddToRoleAsync(user, role);

            var roles = await userManager.GetRolesAsync(user);

            return await jwtService.CreateTokensAsync(
                user.Id,
                user.Email!,
                user.DisplayName ?? user.UserName,
                roles.ToArray(),
                ct);
        }

        public async Task<TokenPairDto> LoginAsync(LoginDto request, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
                throw new InvalidOperationException("Email and password are required.");

            // Нормалізація вводу
            var email = request.Email.Trim();

            // Використовуємо той самий механізм, що й при реєстрації
            var user = await userManager.FindByEmailAsync(email);

            if (user is null)
                throw new InvalidOperationException("Invalid login or password.");

            var signInResult = await signInManager.CheckPasswordSignInAsync(
                user, request.Password, lockoutOnFailure: false);

            if (signInResult.IsLockedOut)
                throw new InvalidOperationException("User is locked out.");

            if (signInResult.IsNotAllowed)
                throw new InvalidOperationException("User is not allowed to sign in (email not confirmed?).");

            if (signInResult.RequiresTwoFactor)
                throw new InvalidOperationException("Two-factor authentication required.");

            if (!signInResult.Succeeded)
                throw new InvalidOperationException("Invalid login or password.");

            var roles = await userManager.GetRolesAsync(user);

            return await jwtService.CreateTokensAsync(
                user.Id,
                user.Email!,
                user.DisplayName ?? user.UserName,
                roles.ToArray(),
                ct);
        }


        public async Task LogoutAsync(LogoutDto request, CancellationToken ct)
        {
            if (!Guid.TryParse(request.UserId, out var userId))
                throw new InvalidOperationException("Invalid user id.");

            if (string.IsNullOrWhiteSpace(request.RefreshToken))
                return;

            var tokenEntity = await db.RefreshTokens
                .FirstOrDefaultAsync(x =>
                    x.UserId == userId &&
                    x.Token == request.RefreshToken &&
                    !x.IsRevoked,
                    ct);

            if (tokenEntity is null)
                return;

            tokenEntity.IsRevoked = true;
            await db.SaveChangesAsync(ct);
        }


        public Task<TokenPairDto> RefreshAsync(RefreshDto request, CancellationToken ct)
            => jwtService.RefreshTokensAsync(request.UserId, request.RefreshToken, ct);
    }
}
