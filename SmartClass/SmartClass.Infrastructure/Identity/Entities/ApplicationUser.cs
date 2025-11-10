using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartClass.Infrastructure.Identity.Entities
{
    public class ApplicationUser : IdentityUser<Guid>
    {
        public string? DisplayName { get; set; }
        public string? Firstname { get; set; }
        public string? Lastname { get; set; }
        public DateTime? BirthDate { get; set; }
    }
}
