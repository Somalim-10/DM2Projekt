using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using DM2Projekt.Data;
using DM2Projekt.Models;

namespace DM2Projekt.Pages.Smartboards
{
    public class DeleteModel : PageModel
    {
        private readonly DM2Projekt.Data.DM2ProjektContext _context;

        public DeleteModel(DM2Projekt.Data.DM2ProjektContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Smartboard Smartboard { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var smartboard = await _context.Smartboard.FirstOrDefaultAsync(m => m.SmartboardId == id);

            if (smartboard is not null)
            {
                Smartboard = smartboard;

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

            var smartboard = await _context.Smartboard.FindAsync(id);
            if (smartboard != null)
            {
                Smartboard = smartboard;
                _context.Smartboard.Remove(Smartboard);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage("./Index");
        }
    }
}
