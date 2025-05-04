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

    public int? CurrentUserId { get; set; } // needed for checks in view

    public List<Booking> UpcomingBookings { get; set; } = new();
    public List<Group> UserGroups { get; set; } = new();

    public async Task<IActionResult> OnGetAsync()
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        var userRole = HttpContext.Session.GetString("UserRole");

        // not logged in? bounce
        if (userId == null)
            return RedirectToPage("/Login");

        // only students allowed here
        if (userRole != "Student")
            return RedirectToPage("/Index");

        CurrentUserId = userId;

        // grab user info
        var user = await _context.User.FirstOrDefaultAsync(u => u.UserId == userId.Value);
        if (user != null)
        {
            UserName = $"{user.FirstName} {user.LastName}";
            UserEmail = user.Email;
            UserRole = user.Role.ToString();
        }

        var now = DateTime.Now;

        // get user's groups
        UserGroups = await _context.UserGroup
            .Where(ug => ug.UserId == userId)
            .Include(ug => ug.Group)
            .Select(ug => ug.Group)
            .ToListAsync();

        var groupIds = UserGroups.Select(g => g.GroupId).ToHashSet();

        // get upcoming bookings where user is involved
        var allUpcoming = await _context.Booking
            .Include(b => b.Room)
            .Include(b => b.Group)
            .Where(b => b.EndTime > now)
            .ToListAsync();

        // filter in memory — chill and safe
        UpcomingBookings = allUpcoming
            .Where(b => b.CreatedByUserId == userId || groupIds.Contains(b.GroupId))
            .OrderBy(b => b.StartTime)
            .ToList();

        return Page();
    }
}
