using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using DM2Projekt.Models;
using Microsoft.AspNetCore.Mvc;

namespace DM2Projekt.Pages.Bookings;

public class IndexModel : PageModel
{
    private readonly Data.DM2ProjektContext _context;

    public IndexModel(Data.DM2ProjektContext context)
    {
        _context = context;
    }

    public IList<Booking> Booking { get; set; } = default!;

    public async Task<IActionResult> OnGetAsync()
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null) // if not logged in
        {
            return RedirectToPage("/Login");
        }

        Booking = await _context.Booking
            .Include(b => b.Group)
            .Include(b => b.Room)
            .Include(b => b.CreatedByUser)
            .OrderBy(b => b.StartTime)
            .ToListAsync();

        return Page();
    }

}
