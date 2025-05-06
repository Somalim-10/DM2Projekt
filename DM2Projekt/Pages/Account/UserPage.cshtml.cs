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

    // handles form input from the password change thing
    [BindProperty]
    public ChangePasswordInputModel Input { get; set; }

    // tracks result messages and state
    public string PasswordChangeMessage { get; set; } = "";
    public bool PasswordChangeSuccess { get; set; } = false;

    // model for the form fields
    public class ChangePasswordInputModel
    {
        public string CurrentPassword { get; set; }
        public string NewPassword { get; set; }
        public string ConfirmPassword { get; set; }
    }

    // used to show relative time like "in 2 days"
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
        // make sure you're logged in and a student
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

    public async Task<IActionResult> OnPostAsync()
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null)
            return RedirectToPage("/Login");

        var user = await _context.User.FirstOrDefaultAsync(u => u.UserId == userId);
        if (user == null)
        {
            PasswordChangeMessage = "User not found. Weird.";
            await LoadAllUserData(userId.Value); // <--- makes sure UI doesn't break
            return Page();
        }

        // check for empty fields
        if (string.IsNullOrWhiteSpace(Input.CurrentPassword) ||
            string.IsNullOrWhiteSpace(Input.NewPassword) ||
            string.IsNullOrWhiteSpace(Input.ConfirmPassword))
        {
            PasswordChangeMessage = "Please fill in all the fields.";
            await LoadAllUserData(userId.Value);
            return Page();
        }

        // current password wrong
        if (Input.CurrentPassword != user.Password)
        {
            PasswordChangeMessage = "Your current password is incorrect.";
            await LoadAllUserData(userId.Value);
            return Page();
        }

        // can't reuse same password
        if (Input.CurrentPassword == Input.NewPassword)
        {
            PasswordChangeMessage = "New password can't be the same as the current one.";
            await LoadAllUserData(userId.Value);
            return Page();
        }

        // new + confirm don't match
        if (Input.NewPassword != Input.ConfirmPassword)
        {
            PasswordChangeMessage = "New passwords don't match.";
            await LoadAllUserData(userId.Value);
            return Page();
        }

        // too short
        if (Input.NewPassword.Length < 6)
        {
            PasswordChangeMessage = "Password should be at least 6 characters.";
            await LoadAllUserData(userId.Value);
            return Page();
        }

        // cool, everything checks out
        user.Password = Input.NewPassword;
        await _context.SaveChangesAsync();

        PasswordChangeSuccess = true;
        PasswordChangeMessage = "Password updated successfully!";
        return await OnGetAsync(); // refreshes cleanly
    }


    private async Task LoadAllUserData(int userId)
    {
        CurrentUserId = userId;
        await LoadUserInfo(userId);
        await LoadUserGroups(userId);
        await LoadUpcomingBookings(userId);
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

        UpcomingBookings = bookings
            .Where(b => b.CreatedByUserId == userId || groupIds.Contains(b.GroupId))
            .OrderBy(b => b.StartTime)
            .ToList();
    }
}
