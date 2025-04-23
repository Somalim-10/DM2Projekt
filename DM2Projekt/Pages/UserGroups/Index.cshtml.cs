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
    public class IndexModel : PageModel
    {
        private readonly DM2Projekt.Data.DM2ProjektContext _context;

        public IndexModel(DM2Projekt.Data.DM2ProjektContext context)
        {
            _context = context;
        }

        public IList<UserGroup> UserGroup { get;set; } = default!;

        public async Task OnGetAsync()
        {
            UserGroup = await _context.UserGroup
                .Include(u => u.Group)
                .Include(u => u.User).ToListAsync();
        }
    }
}
