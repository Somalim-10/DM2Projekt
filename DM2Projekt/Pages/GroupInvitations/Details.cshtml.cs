using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using DM2Projekt.Data;
using DM2Projekt.Models;

namespace DM2Projekt.Pages.GroupInvitations
{
    public class DetailsModel : PageModel
    {
        private readonly DM2Projekt.Data.DM2ProjektContext _context;

        public DetailsModel(DM2Projekt.Data.DM2ProjektContext context)
        {
            _context = context;
        }

        public GroupInvitation GroupInvitation { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var groupinvitation = await _context.GroupInvitation.FirstOrDefaultAsync(m => m.InvitationId == id);

            if (groupinvitation is not null)
            {
                GroupInvitation = groupinvitation;

                return Page();
            }

            return NotFound();
        }
    }
}
