using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using DM2Projekt.Data;
using DM2Projekt.Models;

namespace DM2Projekt.Pages;

public class IndexModel : PageModel
{
    private readonly DM2ProjektContext _context;

    public IndexModel(DM2ProjektContext context)
    {
        _context = context;
    }

    // Student: show their next upcoming booking
    public Booking? NextBooking { get; set; }

    // Admin: total user count
    public int UserCount { get; set; }

    // Teacher: number of bookings they can cancel
    public int CancellableBookingCount { get; set; }

    // Teacher: profile image path
    public string? TeacherProfileImagePath { get; set; }

    public async Task OnGetAsync()
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        var role = HttpContext.Session.GetString("UserRole");
        var now = DateTime.Now;

        if (role == "Student" && userId != null)
        {
            // get all group IDs the student belongs to
            var groupIds = await _context.UserGroup
                .Where(ug => ug.UserId == userId)
                .Select(ug => ug.GroupId)
                .ToListAsync();

            // find the earliest upcoming booking they created or are part of
            var upcoming = await _context.Booking
                .Include(b => b.Room)
                .Include(b => b.Group)
                .Where(b => b.EndTime > now)
                .OrderBy(b => b.StartTime)
                .ToListAsync();

            NextBooking = upcoming
                .FirstOrDefault(b =>
                    b.CreatedByUserId == userId || groupIds.Contains(b.GroupId));
        }

        if (role == "Teacher" && userId != null)
        {
            // bookings that start at least 3 days from now
            var cutoff = now.AddDays(3);
            CancellableBookingCount = await _context.Booking
                .CountAsync(b => b.StartTime > cutoff);

            // teacher's profile photo
            var teacher = await _context.User.FirstOrDefaultAsync(u => u.UserId == userId);
            TeacherProfileImagePath = teacher?.ProfileImagePath;
        }

        if (role == "Admin")
        {
            UserCount = await _context.User.CountAsync();
        }
    }

    // Utility for "Starts in X minutes/hours"
    public static string GetRelativeTime(DateTime target)
    {
        var now = DateTime.Now;
        var diff = target - now;

        if (diff.TotalMinutes < 1)
            return "right now";

        var hours = (int)diff.TotalHours;
        var minutes = diff.Minutes;

        var parts = new List<string>();
        if (hours > 0) parts.Add($"{hours} hour{(hours > 1 ? "s" : "")}");
        if (minutes > 0) parts.Add($"{minutes} minute{(minutes > 1 ? "s" : "")}");

        return "Starts in " + string.Join(" and ", parts);
    }
}
