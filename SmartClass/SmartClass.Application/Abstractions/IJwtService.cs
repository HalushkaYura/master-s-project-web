

using System.Security.Claims;

namespace SmartClass.Application.Abstractions
{
    public interface IJwtService
    {
        //IEnumerable<Claim> SetClaims(ApplicationApplicationUser ApplicationUser);
        string CreateToken(IEnumerable<Claim> claims);
        string CreateRefreshToken();
        IEnumerable<Claim> GetClaimsFromExpiredToken(string token);
       // Task<GoogleJsonWebSignature.Payload> VerifyGoogleToken(ApplicationUserExternalAuthDTO authDTO);
    }
}
