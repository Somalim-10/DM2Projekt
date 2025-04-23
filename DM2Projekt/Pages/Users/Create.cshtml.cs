using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using DM2Projekt.Data;
using DM2Projekt.Models;
using DM2Projekt.Models.Enums;

namespace DM2Projekt.Pages.Users
{
    public class CreateModel : PageModel
    {
        public IEnumerable<SelectListItem> RoleOptions { get; set; } = [];


        private readonly DM2Projekt.Data.DM2ProjektContext _context;

        public CreateModel(DM2Projekt.Data.DM2ProjektContext context)
        {
            _context = context;
        }

        public IActionResult OnGet()
        {
            RoleOptions = Enum.GetValues(typeof(Role))
                              .Cast<Role>()
                              .Select(r => new SelectListItem
                              {
                                  Value = ((int)r).ToString(),
                                  Text = r.ToString()
                              });

            return Page();
        }

        [BindProperty]
        public User User { get; set; } = default!;

        // For more information, see https://aka.ms/RazorPagesCRUD.
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            _context.User.Add(User);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
}


// FJERN DET HER, DET FORKERT Lol