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
    public class IndexModel : PageModel
    {
        private readonly DM2Projekt.Data.DM2ProjektContext _context;

        public IndexModel(DM2Projekt.Data.DM2ProjektContext context)
        {
            _context = context;
        }

        public IList<Smartboard> Smartboard { get;set; } = default!;

        public async Task OnGetAsync()
        {
            Smartboard = await _context.Smartboard
                .Include(s => s.Room).ToListAsync();
        }
    }
}
