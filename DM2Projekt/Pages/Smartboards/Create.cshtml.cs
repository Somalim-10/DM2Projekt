using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using DM2Projekt.Data;
using DM2Projekt.Models;

namespace DM2Projekt.Pages.Smartboards
{
    public class CreateModel : PageModel
    {
        private readonly DM2Projekt.Data.DM2ProjektContext _context;

        public CreateModel(DM2Projekt.Data.DM2ProjektContext context)
        {
            _context = context;
        }

        public IActionResult OnGet()
        {
        ViewData["RoomId"] = new SelectList(_context.Room, "RoomId", "RoomId");
            return Page();
        }

        [BindProperty]
        public Smartboard Smartboard { get; set; } = default!;

        // For more information, see https://aka.ms/RazorPagesCRUD.
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            _context.Smartboard.Add(Smartboard);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
}
