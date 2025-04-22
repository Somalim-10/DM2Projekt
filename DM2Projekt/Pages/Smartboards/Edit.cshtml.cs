using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using DM2Projekt.Data;
using DM2Projekt.Models;

namespace DM2Projekt.Pages.Smartboards
{
    public class EditModel : PageModel
    {
        private readonly DM2Projekt.Data.DM2ProjektContext _context;

        public EditModel(DM2Projekt.Data.DM2ProjektContext context)
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

            var smartboard =  await _context.Smartboard.FirstOrDefaultAsync(m => m.SmartboardId == id);
            if (smartboard == null)
            {
                return NotFound();
            }
            Smartboard = smartboard;
           ViewData["RoomId"] = new SelectList(_context.Room, "RoomId", "RoomId");
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

            _context.Attach(Smartboard).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!SmartboardExists(Smartboard.SmartboardId))
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

        private bool SmartboardExists(int id)
        {
            return _context.Smartboard.Any(e => e.SmartboardId == id);
        }
    }
}
