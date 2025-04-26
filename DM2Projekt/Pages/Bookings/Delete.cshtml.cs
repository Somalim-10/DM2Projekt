using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using DM2Projekt.Data;
using DM2Projekt.Models;

namespace DM2Projekt.Pages.Bookings;

public class DeleteModel : PageModel
{
    private readonly DM2ProjektContext _context;

    public DeleteModel(DM2ProjektContext context)
    {
        _context = context;
    }

    [BindProperty]
    public Booking Booking { get; set; } = default!;

    public async Task<IActionResult> OnGetAsync(int? id)
    {
        if (id == null) return NotFound();

        var booking = await _context.Booking
            .Include(b => b.Room)
            .Include(b => b.Group)
            .Include(b => b.CreatedByUser)
            .FirstOrDefaultAsync(m => m.BookingId == id);

        if (booking == null) return NotFound();

        var userId = HttpContext.Session.GetInt32("UserId");
        var userRole = HttpContext.Session.GetString("UserRole");

        if (userId == null) return RedirectToPage("/Login");

        // only Admins, Teachers, or the person who created it can delete
        if (userRole != "Admin" && userRole != "Teacher" && booking.CreatedByUserId != userId)
        {
            TempData["ErrorMessage"] = "You are not allowed to cancel this booking.";
            return RedirectToPage("/Bookings/Index");
        }

        Booking = booking;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int? id)
    {
        if (id == null) return NotFound();

        var booking = await _context.Booking
            .Include(b => b.CreatedByUser)
            .FirstOrDefaultAsync(b => b.BookingId == id);

        if (booking == null) return NotFound();

        var userId = HttpContext.Session.GetInt32("UserId");
        var userRole = HttpContext.Session.GetString("UserRole");

        if (userId == null) return RedirectToPage("/Login");

        // only Admins, Teachers, or the person who created it can delete
        if (userRole != "Admin" && userRole != "Teacher" && booking.CreatedByUserId != userId)
        {
            TempData["ErrorMessage"] = "You are not allowed to cancel this booking.";
            return RedirectToPage("/Bookings/Index");
        }

        // teachers can only cancel if more than 3 days before start
        if (userRole == "Teacher")
        {
            if (booking.StartTime.HasValue)
            {
                var now = DateTime.Now;
                var diff = booking.StartTime.Value - now;

                if (diff.TotalDays < 3)
                {
                    // not enough notice, show message
                    TempData["ErrorMessage"] = "You can only cancel bookings more than 3 days before they start.";
                    return RedirectToPage("/Bookings/Index");
                }
            }
        }

        // delete the booking
        _context.Booking.Remove(booking);
        await _context.SaveChangesAsync();

        return RedirectToPage("./Index");
    }
}
