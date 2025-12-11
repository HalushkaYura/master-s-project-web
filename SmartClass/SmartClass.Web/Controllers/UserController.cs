using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartClass.Application.Abstractions;
using SmartClass.Application.Contracts.User;
using System.Security.Claims;

namespace SmartClass.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private string? UserId => User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }


        [HttpPut]
        [Route("edit")]
        public async Task<IActionResult> EditUserDateAsync([FromBody] UserEditDTO userEditDTO)
        {
            if (string.IsNullOrEmpty(UserId))
            {
                return Unauthorized("User ID claim is missing.");
            }
            await _userService.EditUserDateAsync(userEditDTO, UserId);

            return Ok();
        }

        [HttpGet]
        [Route("info")]
        public async Task<IActionResult> GetUserInfoAsync()
        {
            if (string.IsNullOrEmpty(UserId))
            {
                return Unauthorized("User ID claim is missing.");
            }

                var userInfo = await _userService.UserInfoAsync(UserId);

            if (userInfo == null)
            {
                return NotFound("User not found.");
            }

            return Ok(userInfo);
        }




        [Authorize(Roles = "Teacher")]
        [HttpGet("teacher-only")]
        public IActionResult OnlyForTeachers()
        {
            return Ok(new { ok = true, message = "You are a Teacher" });
        }
        ////////////////////////////////////////////////////////////////////////////////


        /*      
                   [HttpPut]
                   [Route("upload-foto")]
                   public async Task<IActionResult> UploadUserImage([FromForm] UserImageUploadDTO imageDTO)
                   {
                       await _userService.UploadAvatar(imageDTO, UserId);
                       return Ok();

                   }
               [HttpGet]
               [Route("get-image/{email}")]
               public async Task<IActionResult> GetUserImage(string email)
               {
                   var imageUrl = await _userService.GetUserImageAsync(email);
                   return Ok(imageUrl);
               }
               [HttpDelete]
               [Route("delete")]
               public async Task<IActionResult> DeleteUserAccount()
               {
                   await _userService.DeleteUserAccount(UserId);

                   return Ok();
               }*/

    }
}
