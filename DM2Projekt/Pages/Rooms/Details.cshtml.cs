using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using DM2Projekt.Data;
using DM2Projekt.Models;

namespace DM2Projekt.Pages.Rooms;

public class DetailsModel : PageModel
{
    private readonly DM2ProjektContext _context;

    public DetailsModel(DM2ProjektContext context)
    {
        _context = context;
    }

    public Room Room { get; set; } = default!;

    public async Task<IActionResult> OnGetAsync(int? id)
    {
        var userId = HttpContext.Session.GetInt32("UserId");

        // must be logged in to view details
        if (userId == null)
            return RedirectToPage("/Login");

        if (id == null)
            return NotFound();

        var room = await _context.Room.FirstOrDefaultAsync(m => m.RoomId == id);

        if (room == null)
            return NotFound();

        Room = room;
        return Page();
    }
}
