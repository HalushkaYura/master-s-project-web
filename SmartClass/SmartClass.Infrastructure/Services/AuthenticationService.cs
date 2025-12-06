using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using SmartClass.Application.Abstractions;
using SmartClass.Application.Contracts.User;
using SmartClass.Domain.Exceptions;
using SmartClass.Infrastructure.Helpers.Mails;
using SmartClass.Infrastructure.Identity.Entities;
using System.Net;
using System.Text;
namespace SmartClass.Infrastructure.Services
{
    public class AuthenticationService : IAuthenticationService
    {
        /*private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IJwtService _jwtService;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IRepository<RefreshToken> _refreshTokenRepository;
        private readonly IEmailSenderService _emailSenderService;
        private readonly IConfirmEmailService _confirmEmailService;
        private readonly ITemplateService _templateService;
        private readonly IOptions<ClientUrl> _clientUrl;

        public AuthenticationService(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IJwtService jwtService,
            RoleManager<IdentityRole> roleManager,
            IRepository<RefreshToken> refreshTokenRepository,
            IEmailSenderService emailSenderService,
            IConfirmEmailService confirmEmailService,
            ITemplateService templateService,
            IOptions<ClientUrl> options)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _jwtService = jwtService;
            _roleManager = roleManager;
            _refreshTokenRepository = refreshTokenRepository;
            _emailSenderService = emailSenderService;
            _confirmEmailService = confirmEmailService;
            _templateService = templateService;
            _clientUrl = options;
        }

        // ---------- LOGIN ----------
        public async Task<UserAuthorizationDTO> LoginAsync(string email, string password)
        {
            var user = await _userManager.FindByEmailAsync(email);

            if (user == null || !await _userManager.CheckPasswordAsync(user, password))
            {
                throw new HttpException(HttpStatusCode.Unauthorized, "Incorrect login or password");
            }

            if (await _userManager.GetTwoFactorEnabledAsync(user))
            {
                return await GenerateTwoStepVerificationCode(user);
            }

            return await GenerateUserTokens(user);
        }

        // ---------- LOGOUT ----------
        public async Task LogoutAsync(UserAuthorizationDTO userTokensDTO)
        {
           // var specification = new RefreshTokens.SearchRefreshToken(userTokensDTO.RefreshToken);
            var refreshTokenFromDb = await _refreshTokenRepository.GetFirstBySpecAsync(specification);

            if (refreshTokenFromDb == null)
            {
                return;
            }

            await _refreshTokenRepository.DeleteAsync(refreshTokenFromDb);
            await _refreshTokenRepository.SaveChangesAsync();
        }

        // ---------- REGISTRATION ----------
        public async Task RegistrationAsync(ApplicationUser user, string password, string roleName)
        {
            var result = await _userManager.CreateAsync(user, password);

            if (!result.Succeeded)
            {
                var sb = new StringBuilder();
                foreach (var error in result.Errors)
                {
                    sb.Append(error.Description).Append(' ');
                }
                throw new HttpException(HttpStatusCode.BadRequest, sb.ToString());
            }

            var findRole = await _roleManager.FindByNameAsync(roleName);

            if (findRole == null)
            {
                await _roleManager.CreateAsync(new IdentityRole(roleName));
            }

            await _userManager.AddToRoleAsync(user, roleName);
        }

        // (Далі можеш перенести/спростити SentResetPasswordTokenAsync, ResetPasswordAsync,
        // ExternalLoginAsync, LoginTwoStepAsync, RefreshTokenAsync, ChangePasswordAsync
        // — код у тебе вже є, просто онови неймспейси + DTO/Exception.)

        private async Task<UserAuthorizationDTO> GenerateTwoStepVerificationCode(ApplicationUser user)
        {
            var providers = await _userManager.GetValidTwoFactorProvidersAsync(user);

            if (!providers.Contains("Email"))
            {
                throw new HttpException(HttpStatusCode.Unauthorized, "Invalid 2-step configuration");
            }

            var twoFactorToken = await _userManager.GenerateTwoFactorTokenAsync(user, "Email");
            var message = new MailRequest
            {
                ToEmail = user.Email!,
                Subject = "SmartClass authentication code",
                Body = await _templateService.GetTemplateHtmlAsStringAsync("Mails/TwoFactorCode",
                 /*   new UserToken
                    {
                        Token = twoFactorToken,
                        UserName = user.UserName!,
                        Uri = _clientUrl.Value.ApplicationUrl
                    })
            };

            //await _emailSenderService.SendEmailAsync(message);

            return new UserAuthorizationDTO
            {
                Is2StepVerificationRequired = true,
                Provider = "Email"
            };
        }

        private async Task<UserAuthorizationDTO> GenerateUserTokens(ApplicationUser user)
        {
            //var claims = _jwtService.SetClaims(user);
            var token = _jwtService.CreateToken(claims);
            var refreshToken = await CreateRefreshToken(user);

            return new UserAuthorizationDTO
            {
                Token = token,
                RefreshToken = refreshToken
            };
        }

        private async Task<string> CreateRefreshToken(ApplicationUser user)
        {
            var refreshToken = _jwtService.CreateRefreshToken();

            var rt = new RefreshToken
            {
                Token = refreshToken,
                UserId = user.Id
            };

            await _refreshTokenRepository.AddAsync(rt);
            await _refreshTokenRepository.SaveChangesAsync();

            return refreshToken;
        }*/

        // ...додай інші методи з твого старого класу при потребі...
    }
}
