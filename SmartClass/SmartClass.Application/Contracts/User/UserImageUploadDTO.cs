using Microsoft.AspNetCore.Http;

namespace SmartClass.Application.Contracts.User
{
    public class UserImageUploadDTO
    {
        public IFormFile Image { get; set; }
    }
}
