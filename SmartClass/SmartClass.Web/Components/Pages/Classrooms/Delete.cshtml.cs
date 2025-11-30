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
    public class DeleteModel : PageModel
    {
        private readonly SmartClass.Infrastructure.Persistence.AppDbContext _context;

        public DeleteModel(SmartClass.Infrastructure.Persistence.AppDbContext context)
        {
            _context = context;
        }

        [BindProperty]
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

        public async Task<IActionResult> OnPostAsync(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var classroom = await _context.Classrooms.FindAsync(id);
            if (classroom != null)
            {
                Classroom = classroom;
                _context.Classrooms.Remove(Classroom);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage("./Index");
        }
    }
}
