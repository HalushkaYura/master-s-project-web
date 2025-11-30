using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SmartClass.Domain.Entities;
using SmartClass.Infrastructure.Persistence;

namespace SmartClass.Web.Components.Pages
{
    public class IndexModel : PageModel
    {
        private readonly SmartClass.Infrastructure.Persistence.AppDbContext _context;

        public IndexModel(SmartClass.Infrastructure.Persistence.AppDbContext context)
        {
            _context = context;
        }

        public IList<Classroom> Classroom { get;set; } = default!;

        public async Task OnGetAsync()
        {
            Classroom = await _context.Classrooms.ToListAsync();
        }
    }
}
