using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartClass.Application.Contracts.Auth
{
    public record LogoutDto(string UserId, string RefreshToken);

}
