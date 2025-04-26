using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using DM2Projekt.Data;
using DM2Projekt.Models;

namespace DM2Projekt.Pages.Rooms;

public class EditModel : PageModel
{
    private readonly DM2ProjektContext _context;

    public EditModel(DM2ProjektContext context)
    {
        _context = context;
    }

    [BindProperty]
    public Room Room { get; set; } = default!;

    public async Task<IActionResult> OnGetAsync(int? id)
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        var userRole = HttpContext.Session.GetString("UserRole");

        // must be logged in and be Admin
        if (userId == null || userRole != "Admin")
            return RedirectToPage("/Login");

        if (id == null)
            return NotFound();

        var room = await _context.Room.FirstOrDefaultAsync(m => m.RoomId == id);
        if (room == null)
            return NotFound();

        Room = room;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        var userRole = HttpContext.Session.GetString("UserRole");

        // must be logged in and be Admin
        if (userId == null || userRole != "Admin")
            return RedirectToPage("/Login");

        if (!ModelState.IsValid)
            return Page();

        _context.Attach(Room).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!RoomExists(Room.RoomId))
                return NotFound();
            else
                throw;
        }

        return RedirectToPage("./Index");
    }

    // helper to check if room still exists
    private bool RoomExists(int id)
    {
        return _context.Room.Any(e => e.RoomId == id);
    }
}
