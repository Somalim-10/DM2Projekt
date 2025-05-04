using DM2Projekt.Data;
using DM2Projekt.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace DM2Projekt.Pages.Account;

public class UserPageModel : PageModel
{
    private readonly DM2ProjektContext _context;

    public UserPageModel(DM2ProjektContext context)
    {
        _context = context;
    }

    public string UserName { get; set; }
    public string UserEmail { get; set; }
    public int? CurrentUserId { get; set; }

    public List<Booking> UpcomingBookings { get; set; } = new();
    public List<Group> UserGroups { get; set; } = new();

    // Show things like: "in 2 days", "tomorrow"
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
        // must be logged in and a student
        var userId = HttpContext.Session.GetInt32("UserId");
        var userRole = HttpContext.Session.GetString("UserRole");

        if (userId == null)
            return RedirectToPage("/Login");

        if (userRole != "Student")
            return RedirectToPage("/Index");

        CurrentUserId = userId;

        await LoadUserInfo(userId.Value);
        await LoadUserGroups(userId.Value);
        await LoadUpcomingBookings(userId.Value);

        return Page();
    }

    private async Task LoadUserInfo(int userId)
    {
        var user = await _context.User.FirstOrDefaultAsync(u => u.UserId == userId);

        if (user != null)
        {
            UserName = $"{user.FirstName} {user.LastName}";
            UserEmail = user.Email;
        }
    }

    private async Task LoadUserGroups(int userId)
    {
        UserGroups = await _context.UserGroup
            .Where(ug => ug.UserId == userId)
            .Include(ug => ug.Group)
            .Select(ug => ug.Group)
            .ToListAsync();
    }

    private async Task LoadUpcomingBookings(int userId)
    {
        var now = DateTime.Now;
        var groupIds = UserGroups.Select(g => g.GroupId).ToHashSet();

        var bookings = await _context.Booking
            .Include(b => b.Room)
            .Include(b => b.Group)
            .Where(b => b.EndTime > now)
            .ToListAsync();

        // filter in memory (safe for joins)
        UpcomingBookings = bookings
            .Where(b => b.CreatedByUserId == userId || groupIds.Contains(b.GroupId))
            .OrderBy(b => b.StartTime)
            .ToList();
    }
}
