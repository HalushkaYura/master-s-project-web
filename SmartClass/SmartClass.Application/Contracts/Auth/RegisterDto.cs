namespace SmartClass.Application.Contracts.Auth
{
    public class RegisterDto
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;

        public DateTime BirthDate { get; set; }

        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;

        // "Teacher" / "Student"
        public string Role { get; set; } = "Student";
    }
}
