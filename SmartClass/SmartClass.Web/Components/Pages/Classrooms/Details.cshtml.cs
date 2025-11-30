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
    public class DetailsModel : PageModel
    {
        private readonly SmartClass.Infrastructure.Persistence.AppDbContext _context;

        public DetailsModel(SmartClass.Infrastructure.Persistence.AppDbContext context)
        {
            _context = context;
        }

        public Classroom Classroom { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var classroom = await _context.Classrooms.FirstOrDefaultAsync(m => m.Id == id);
            if (classroom == null)
            {
                return NotFound();
            }
            else
            {
                Classroom = classroom;
            }
            return Page();
        }
    }
}
