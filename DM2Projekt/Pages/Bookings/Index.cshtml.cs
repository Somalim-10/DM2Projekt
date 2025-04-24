using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using DM2Projekt.Data;
using DM2Projekt.Models;

namespace DM2Projekt.Pages.Bookings
{
    public class IndexModel : PageModel
    {
        private readonly DM2Projekt.Data.DM2ProjektContext _context;

        public IndexModel(DM2Projekt.Data.DM2ProjektContext context)
        {
            _context = context;
        }

        public IList<Booking> Booking { get; set; } = default!;

        public async Task OnGetAsync()
        {
            Booking = await _context.Booking
                .Include(b => b.Group)
                .Include(b => b.Room)
                .Include(b => b.Smartboard)
                .Include(b => b.CreatedByUser)
                .OrderBy(b => b.StartTime)
                .ToListAsync();
        }

    }
}
