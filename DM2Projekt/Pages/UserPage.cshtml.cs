using DM2Projekt.Data;
using DM2Projekt.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace DM2Projekt.Pages;

public class UserPageModel : PageModel
{
    private readonly DM2ProjektContext _context;

    public UserPageModel(DM2ProjektContext context)
    {
        _context = context;
    }

    public string UserName { get; set; }
    public string UserEmail { get; set; }
    public string UserRole { get; set; }

    public int? CurrentUserId { get; set; } // for use in Razor checks

    public List<Booking> UpcomingBookings { get; set; } = new();
    public List<Group> UserGroups { get; set; } = new();

    // shows "in 2 days", "tomorrow", etc.
    public static string GetRelativeTime(DateTime time)
    {
        var now = DateTime.Now;
        var span = time.Date - now.Date;

        if (span.TotalDays == 0) return "today";
        if (span.TotalDays == 1) return "tomorrow";
        if (span.TotalDays < 7) return $"in {(int)span.TotalDays} days";
        return time.ToString("d MMM");
    }

    public async Task<IActionResult> OnGetAsync()
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        var userRole = HttpContext.Session.GetString("UserRole");

        // bounce if not logged in
        if (userId == null)
            return RedirectToPage("/Login");

        // only students allowed
        if (userRole != "Student")
            return RedirectToPage("/Index");

        CurrentUserId = userId;

        // basic user info
        var user = await _context.User.FirstOrDefaultAsync(u => u.UserId == userId.Value);
        if (user != null)
        {
            UserName = $"{user.FirstName} {user.LastName}";
            UserEmail = user.Email;
            UserRole = user.Role.ToString();
        }

        var now = DateTime.Now;

        // get all groups user is part of
        UserGroups = await _context.UserGroup
            .Where(ug => ug.UserId == userId)
            .Include(ug => ug.Group)
            .Select(ug => ug.Group)
            .ToListAsync();

        var groupIds = UserGroups.Select(g => g.GroupId).ToHashSet();

        // get upcoming bookings (created by user or for a group they are in)
        var allUpcoming = await _context.Booking
            .Include(b => b.Room)
            .Include(b => b.Group)
            .Where(b => b.EndTime > now)
            .ToListAsync();

        // in-memory filter (avoids EF weirdness)
        UpcomingBookings = allUpcoming
            .Where(b => b.CreatedByUserId == userId || groupIds.Contains(b.GroupId))
            .OrderBy(b => b.StartTime)
            .ToList();

        return Page();
    }
}
