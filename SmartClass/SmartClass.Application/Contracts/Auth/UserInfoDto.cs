namespace SmartClass.Application.Contracts.Auth
{
    public class UserInfoDto
    {
        public string UserId { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Firstname { get; set; } = string.Empty;
        public string Lastname { get; set; } = string.Empty;
        public DateTime BirthDate { get; set; } = DateTime.MinValue;
        public string[] Roles { get; set; } = Array.Empty<string>();
    }
}
