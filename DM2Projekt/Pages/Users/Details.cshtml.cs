using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using DM2Projekt.Data;
using DM2Projekt.Models;

namespace DM2Projekt.Pages.Users;

// shows full profile info for one user (admin only)
public class DetailsModel : PageModel
{
    private readonly DM2ProjektContext _context;

    public DetailsModel(DM2ProjektContext context)
    {
        _context = context;
    }

    public User User { get; set; } = default!;

    public async Task<IActionResult> OnGetAsync(int? id)
    {
        if (id == null) return NotFound();

        var userId = HttpContext.Session.GetInt32("UserId");
        var userRole = HttpContext.Session.GetString("UserRole");

        // bounce if not logged in or not admin
        if (userId == null) return RedirectToPage("/Login");
        if (userRole != "Admin") return RedirectToPage("/Index");

        // load user + their groups + their bookings (including rooms)
        var user = await _context.User
            .Include(u => u.UserGroups)
                .ThenInclude(ug => ug.Group)
            .Include(u => u.Bookings)
                .ThenInclude(b => b.Room)
            .FirstOrDefaultAsync(u => u.UserId == id);

        if (user == null) return NotFound();

        User = user;
        return Page();
    }
}
