using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using DM2Projekt.Models;

namespace DM2Projekt.Pages.Bookings;

public class IndexModel : PageModel
{
    private readonly Data.DM2ProjektContext _context;

    public IndexModel(Data.DM2ProjektContext context)
    {
        _context = context;
    }

    public IList<Booking> Booking { get; set; } = default!;

    // --- filter stuff here ---
    public List<SelectListItem> RoomOptions { get; set; } = new(); // holds all room choices

    [BindProperty(SupportsGet = true)]
    public int? RoomId { get; set; } // selected room from URL

    public int? SelectedRoomId => RoomId; // helper to make it nicer in frontend
    // --- end filter stuff ---

    public async Task<IActionResult> OnGetAsync()
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null) // not logged in? get out
        {
            return RedirectToPage("/Login");
        }

        // grab all rooms for dropdown
        RoomOptions = await _context.Room
            .OrderBy(r => r.RoomName)
            .Select(r => new SelectListItem
            {
                Value = r.RoomId.ToString(),
                Text = r.RoomName
            })
            .ToListAsync();

        // start the bookings query
        var query = _context.Booking
            .Include(b => b.Group)
            .Include(b => b.Room)
            .Include(b => b.CreatedByUser)
            .AsQueryable();

        // if a room is selected, filter it
        if (RoomId.HasValue)
        {
            query = query.Where(b => b.RoomId == RoomId.Value);
        }

        // finally get the bookings
        Booking = await query
            .OrderBy(b => b.StartTime)
            .AsNoTracking()
            .ToListAsync();

        return Page();
    }
}
