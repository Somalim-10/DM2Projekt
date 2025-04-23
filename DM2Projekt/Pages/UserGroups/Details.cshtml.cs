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
    public class DetailsModel : PageModel
    {
        private readonly DM2Projekt.Data.DM2ProjektContext _context;

        public DetailsModel(DM2Projekt.Data.DM2ProjektContext context)
        {
            _context = context;
        }

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
    }
}
