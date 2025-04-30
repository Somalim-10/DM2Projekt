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

namespace DM2Projekt.Pages.GroupInvitations
{
    public class EditModel : PageModel
    {
        private readonly DM2Projekt.Data.DM2ProjektContext _context;

        public EditModel(DM2Projekt.Data.DM2ProjektContext context)
        {
            _context = context;
        }

        [BindProperty]
        public GroupInvitation GroupInvitation { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var groupinvitation =  await _context.GroupInvitation.FirstOrDefaultAsync(m => m.InvitationId == id);
            if (groupinvitation == null)
            {
                return NotFound();
            }
            GroupInvitation = groupinvitation;
           ViewData["GroupId"] = new SelectList(_context.Group, "GroupId", "GroupName");
           ViewData["InvitedUserId"] = new SelectList(_context.User, "UserId", "Email");
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

            _context.Attach(GroupInvitation).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!GroupInvitationExists(GroupInvitation.InvitationId))
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

        private bool GroupInvitationExists(int id)
        {
            return _context.GroupInvitation.Any(e => e.InvitationId == id);
        }
    }
}
