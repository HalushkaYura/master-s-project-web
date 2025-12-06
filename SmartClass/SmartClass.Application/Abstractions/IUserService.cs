using SmartClass.Application.Contracts.Auth;
using SmartClass.Application.Contracts.User;

namespace SmartClass.Application.Abstractions
{
    public interface IUserService
    {
        Task<UserInfoDTO> UserInfoAsync(string userId);
        Task EditUserDateAsync(UserEditDTO userEditDTO, string userId);
        //Task<List<UserInviteInfoDTO>> GetUserInviteInfoListAsync(string userId);
        //Task<UserActiveInviteDTO> IsActiveInviteAsync(string userId);
        //Task ChangeTwoFactorVerificationStatusAsync(string userId, UserChange2faStatusDTO statusDTO);
        //Task<bool> CheckIsTwoFactorVerificationAsync(string userId);
        //Task SendTwoFactorCodeAsync(string userId);
        Task SetPasswordAsync(string userId, UserSetPasswordDTO userSetPasswordDTO);
        Task<bool> IsHavePasswordAsync(string userId);

        //----------------------------------------------------------
        //Task UploadAvatar(UserImageUploadDTO imageDTO, string userId);
        //Task<string> GetUserImageAsync(string userId);
        //Task<byte[]> GetUserImageAsync(string email);
        //Task DeleteUserAccount(string userId);

    }
}
