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

        var booking = await GetBookingDetailsAsync(id.Value);
        if (booking == null) return NotFound();

        if (!UserHasAccess(booking)) return RedirectToLoginOrIndex();

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

        if (!UserHasAccess(booking)) return RedirectToLoginOrIndex();

        if (IsTeacherTooLate(booking))
        {
            TempData["ErrorMessage"] = "You can only cancel bookings more than 3 days before they start.";
            return RedirectToPage("/Bookings/Index");
        }

        _context.Booking.Remove(booking);
        await _context.SaveChangesAsync();

        return RedirectToPage("./Index");
    }

    // === Private helpers ===

    private async Task<Booking?> GetBookingDetailsAsync(int id)
    {
        return await _context.Booking
            .Include(b => b.Room)
            .Include(b => b.Group)
            .Include(b => b.CreatedByUser)
            .FirstOrDefaultAsync(m => m.BookingId == id);
    }

    private bool UserHasAccess(Booking booking)
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        var role = HttpContext.Session.GetString("UserRole");

        if (userId == null) return false;

        var isAdmin = role == "Admin";
        var isTeacher = role == "Teacher";
        var isOwner = booking.CreatedByUserId == userId;

        return isAdmin || isTeacher || isOwner;
    }

    private bool IsTeacherTooLate(Booking booking)
    {
        var role = HttpContext.Session.GetString("UserRole");
        if (role != "Teacher" || !booking.StartTime.HasValue) return false;

        var daysUntilStart = (booking.StartTime.Value - DateTime.Now).TotalDays;
        return daysUntilStart < 3;
    }

    private IActionResult RedirectToLoginOrIndex()
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null) return RedirectToPage("/Login");

        TempData["ErrorMessage"] = "You are not allowed to cancel this booking.";
        return RedirectToPage("/Bookings/Index");
    }
}
