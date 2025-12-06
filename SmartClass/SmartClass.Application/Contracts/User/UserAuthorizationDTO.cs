namespace SmartClass.Application.Contracts.User
{
    public class  UserAuthorizationDTO
    {
        public string Token { get; set; }
        public string RefreshToken { get; set; }
        public string Provider { get; set; }
        public bool Is2StepVerificationRequired { get; set; }
    }
}
