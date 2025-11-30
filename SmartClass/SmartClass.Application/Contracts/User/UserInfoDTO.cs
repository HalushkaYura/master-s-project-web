using System.Globalization;

namespace SmartClass.Application.Contracts.User
{
    public class UserInfoDTO
    {
        public Guid UserId { get; set; }
        public string Firstname { get; set; }
        public string Lastname { get; set; }
        public DateTime BirthDate { get; set; }
    }
}
