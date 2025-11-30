namespace SmartClass.Application.Contracts.User
{
    public class UserSetNewPasswordDTO
    {
        public string CurrentPassword { get; set; }
        public string NewPassword { get; set; }
    }
}
