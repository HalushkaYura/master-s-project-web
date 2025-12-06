using AutoMapper;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using SmartClass.Application.Abstractions;
using SmartClass.Application.Contracts.Auth;
using SmartClass.Application.Contracts.User;
using SmartClass.Application.Options;
using SmartClass.Domain.Exceptions;
using SmartClass.Domain.Resources;
using SmartClass.Infrastructure.Identity.Entities;

namespace SmartClass.Infrastructure.Services
{
    public class UserService : IUserService
    {
        protected readonly UserManager<ApplicationUser> _userManager;
        //protected readonly IEmailSenderService _emailSenderService;
        protected readonly IMapper _mapper;
        private readonly IOptions<ImageSettingsOptions> _imageSettings;
        //protected readonly ITemplateService _templateService;
        protected readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IWebHostEnvironment _webHostEnvironment;


        public UserService(UserManager<ApplicationUser> userManager,
            IMapper mapper,
            IOptions<ImageSettingsOptions> imageSettings,
            IHttpContextAccessor httpContextAccessor,
            IWebHostEnvironment webHostEnvironment)
        {
            _userManager = userManager;
            //_inviteUserRepository = inviteUser;
            _mapper = mapper;
            _imageSettings = imageSettings;
            _httpContextAccessor = httpContextAccessor;
            _webHostEnvironment = webHostEnvironment;
        }


        public async Task<UserInfoDTO> UserInfoAsync(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("userId is null or empty.", nameof(userId));

            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                // Краще явно повідомити, ніж ловити NullReference десь далі
                throw new HttpException(System.Net.HttpStatusCode.NotFound, ErrorMessages.UserNotFound);
            }

            var roles = await _userManager.GetRolesAsync(user);

            var userPersonalInfo = new UserInfoDTO
            {
                UserId = user.Id.ToString(),
                Email = user.Email ?? string.Empty,
                Firstname = user.Firstname ?? string.Empty,
                Lastname = user.Lastname ?? string.Empty,
                BirthDate = user.BirthDate ?? DateTime.MinValue, // якщо BirthDate nullable
                Roles = roles.ToArray()
            };

            return userPersonalInfo;
        }




        //------------------------------   EditUserDate ---------------------------------------
        public async Task EditUserDateAsync(UserEditDTO userEditDTO, string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                // Якщо користувача не знайдено, можливо, ви можете виконати обробку помилки
                throw new HttpException(System.Net.HttpStatusCode.BadRequest, ErrorMessages.UserNotFound);
            }

            user.Firstname = userEditDTO.Firstname;
            user.Lastname = userEditDTO.Lastname;


            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description).ToList();
                throw new Exception($"Помилка при оновленні користувача: {string.Join(", ", errors)}");
            }
        }


        public async Task<bool> CheckIsTwoFactorVerificationAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (!user.EmailConfirmed)
            {
                throw new HttpException(System.Net.HttpStatusCode.BadRequest,
                ErrorMessages.EmailNotConfirm);
            }

            return await _userManager.GetTwoFactorEnabledAsync(user);
        }

        public async Task SetPasswordAsync(string userId, UserSetPasswordDTO userSetPasswordDTO)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (await _userManager.HasPasswordAsync(user))
            {
                throw new HttpException(System.Net.HttpStatusCode.BadRequest, ErrorMessages.PasswordIsExist);
            }

            await _userManager.AddPasswordAsync(user, userSetPasswordDTO.Password);
        }

        public async Task<bool> IsHavePasswordAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            return await _userManager.HasPasswordAsync(user);
        }
    }
}
