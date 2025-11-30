namespace SmartClass.Infrastructure.Options
{
    public class JwtOptions
    {
        public string Issuer { get; set; } = "SmartClass";
        public string Audience { get; set; } = "SmartClass.Web";
        public string Key { get; set; } = string.Empty;   // мінімум 32 символи
        public int AccessTokenMinutes { get; set; } = 60; // час життя access-токена
        public int RefreshTokenDays { get; set; } = 7;    // життя refresh-токена
    }
}
