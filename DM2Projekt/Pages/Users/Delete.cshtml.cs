using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using DM2Projekt.Data;
using DM2Projekt.Models;

namespace DM2Projekt.Pages.Users;

// Handles deleting a user; admin access only
public class DeleteModel : PageModel
{
    private readonly DM2ProjektContext _context;

    public DeleteModel(DM2ProjektContext context)
    {
        _context = context;
    }

    [BindProperty]
    public User User { get; set; } = default!;

    public async Task<IActionResult> OnGetAsync(int? id)
    {
        if (id == null) return NotFound();

        // Check if someone's logged in and has admin powers
        var userId = HttpContext.Session.GetInt32("UserId");
        var userRole = HttpContext.Session.GetString("UserRole");
        if (userId == null || userRole != "Admin")
            return RedirectToPage("/Login");

        // Try to find the user we’re about to delete
        var user = await _context.User.FirstOrDefaultAsync(u => u.UserId == id);
        if (user == null) return NotFound();

        User = user;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int? id)
    {
        if (id == null) return NotFound();

        // Same admin check here
        var userId = HttpContext.Session.GetInt32("UserId");
        var userRole = HttpContext.Session.GetString("UserRole");
        if (userId == null || userRole != "Admin")
            return RedirectToPage("/Login");

        var user = await _context.User.FindAsync(id);
        if (user == null) return NotFound();

        // Wipe out any bookings this user created
        var userBookings = await _context.Booking
            .Where(b => b.CreatedByUserId == id)
            .ToListAsync();
        _context.Booking.RemoveRange(userBookings);

        // Also delete any groups they made, bookings under those groups go automatically
        var userGroups = await _context.Group
            .Where(g => g.CreatedByUserId == id)
            .ToListAsync();
        _context.Group.RemoveRange(userGroups);

        // Finally, nuke the user
        _context.User.Remove(user);
        await _context.SaveChangesAsync();

        return RedirectToPage("./Index");
    }
}
