using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using DM2Projekt.Data;
using DM2Projekt.Models;

namespace DM2Projekt.Pages.UserGroups
{
    public class DeleteModel : PageModel
    {
        private readonly DM2Projekt.Data.DM2ProjektContext _context;

        public DeleteModel(DM2Projekt.Data.DM2ProjektContext context)
        {
            _context = context;
        }

        [BindProperty]
        public UserGroup UserGroup { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var usergroup = await _context.UserGroup.FirstOrDefaultAsync(m => m.UserId == id);

            if (usergroup is not null)
            {
                UserGroup = usergroup;

                return Page();
            }

            return NotFound();
        }

        public async Task<IActionResult> OnPostAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var usergroup = await _context.UserGroup.FindAsync(id);
            if (usergroup != null)
            {
                UserGroup = usergroup;
                _context.UserGroup.Remove(UserGroup);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage("./Index");
        }
    }
}
