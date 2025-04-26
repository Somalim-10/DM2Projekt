using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using DM2Projekt.Data;
using DM2Projekt.Models;

namespace DM2Projekt.Pages.Bookings;

public class DetailsModel : PageModel
{
    private readonly DM2ProjektContext _context;

    public DetailsModel(DM2ProjektContext context)
    {
        _context = context;
    }

    public Booking Booking { get; set; } = default!;

    public async Task<IActionResult> OnGetAsync(int? id)
    {
        if (id == null) return NotFound();

        // load booking with related data
        var booking = await _context.Booking
            .Include(b => b.Room)
            .Include(b => b.Group)
            .Include(b => b.CreatedByUser)
            .FirstOrDefaultAsync(m => m.BookingId == id);

        if (booking == null) return NotFound();

        Booking = booking;
        return Page();
    }
}
