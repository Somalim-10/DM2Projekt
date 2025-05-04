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

    public Booking? NextBooking { get; set; }
    public int TodayBookingCount { get; set; }
    public int UserCount { get; set; }

    public async Task OnGetAsync()
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        var role = HttpContext.Session.GetString("UserRole");
        var now = DateTime.Now;

        if (role == "Student" && userId != null)
        {
            var groupIds = await _context.UserGroup
                .Where(ug => ug.UserId == userId)
                .Select(ug => ug.GroupId)
                .ToListAsync();

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

        if (role == "Teacher")
        {
            TodayBookingCount = await _context.Booking
                .CountAsync(b => b.StartTime.HasValue && b.StartTime.Value.Date == now.Date);
        }

        if (role == "Admin")
        {
            UserCount = await _context.User.CountAsync();
        }
    }

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
