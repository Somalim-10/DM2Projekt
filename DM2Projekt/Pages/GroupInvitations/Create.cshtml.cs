using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using DM2Projekt.Data;
using DM2Projekt.Models;

namespace DM2Projekt.Pages.GroupInvitations
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
        ViewData["GroupId"] = new SelectList(_context.Group, "GroupId", "GroupName");
        ViewData["InvitedUserId"] = new SelectList(_context.User, "UserId", "Email");
            return Page();
        }

        [BindProperty]
        public GroupInvitation GroupInvitation { get; set; } = default!;

        // For more information, see https://aka.ms/RazorPagesCRUD.
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            _context.GroupInvitation.Add(GroupInvitation);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
}
