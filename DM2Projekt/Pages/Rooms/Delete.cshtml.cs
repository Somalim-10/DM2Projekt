using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using DM2Projekt.Data;
using DM2Projekt.Models;

namespace DM2Projekt.Pages.Rooms;

// this page lets Admins delete rooms. risky biz!
public class DeleteModel : PageModel
{
    private readonly DM2ProjektContext _context;

    public DeleteModel(DM2ProjektContext context)
    {
        _context = context;
    }

    [BindProperty]
    public Room Room { get; set; } = default!;

    public async Task<IActionResult> OnGetAsync(int? id)
    {
        var userRole = HttpContext.Session.GetString("UserRole");

        // Admins only, bro
        if (userRole != "Admin")
            return RedirectToPage("/Rooms/Index");

        if (id == null)
            return NotFound();

        var room = await _context.Room.FirstOrDefaultAsync(m => m.RoomId == id);
        if (room == null)
            return NotFound();

        Room = room;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int? id)
    {
        var userRole = HttpContext.Session.GetString("UserRole");

        // still Admins only
        if (userRole != "Admin")
            return RedirectToPage("/Rooms/Index");

        if (id == null)
            return NotFound();

        var room = await _context.Room.FindAsync(id);
        if (room != null)
        {
            Room = room;
            _context.Room.Remove(Room);
            await _context.SaveChangesAsync();
        }

        return RedirectToPage("./Index");
    }
}
