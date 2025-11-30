using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SmartClass.Domain.Entities;
using SmartClass.Infrastructure.Persistence;

namespace SmartClass.Web.Components.Pages
{
    public class EditModel : PageModel
    {
        private readonly SmartClass.Infrastructure.Persistence.AppDbContext _context;

        public EditModel(SmartClass.Infrastructure.Persistence.AppDbContext context)
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

            var classroom =  await _context.Classrooms.FirstOrDefaultAsync(m => m.Id == id);
            if (classroom == null)
            {
                return NotFound();
            }
            Classroom = classroom;
            return Page();
        }

        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more information, see https://aka.ms/RazorPagesCRUD.
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            _context.Attach(Classroom).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ClassroomExists(Classroom.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return RedirectToPage("./Index");
        }

        private bool ClassroomExists(Guid id)
        {
            return _context.Classrooms.Any(e => e.Id == id);
        }
    }
}
