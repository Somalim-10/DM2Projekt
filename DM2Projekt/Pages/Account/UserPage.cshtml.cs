using DM2Projekt.Data;
using DM2Projekt.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace DM2Projekt.Pages.Account;

/// <summary>
/// Handles logic for the User Profile page (password, picture, bookings, etc.)
/// </summary>
public class UserPageModel : PageModel
{
    private readonly DM2ProjektContext _context;

    public UserPageModel(DM2ProjektContext context)
    {
        _context = context;
    }

    // 🧑 Basic Info
    public string UserName { get; set; }
    public string UserEmail { get; set; }
    public string? ProfileImagePath { get; set; }
    public int? CurrentUserId { get; set; }

    // 📅 Page Data
    public List<Booking> UpcomingBookings { get; set; } = new();
    public List<Models.Group> UserGroups { get; set; } = new();

    // 🛠️ Bindable Form Models
    [BindProperty]
    public ChangePasswordInputModel Input { get; set; }

    [BindProperty]
    public string? NewProfileImageUrl { get; set; }

    // 🔐 Feedback messages
    public string PasswordChangeMessage { get; set; } = "";
    public bool PasswordChangeSuccess { get; set; } = false;

    public string ProfilePictureMessage { get; set; } = "";
    public bool ProfilePictureSuccess { get; set; } = false;

    /// <summary>
    /// View model for password change
    /// </summary>
    public class ChangePasswordInputModel
    {
        public string CurrentPassword { get; set; }
        public string NewPassword { get; set; }
        public string ConfirmPassword { get; set; }
    }

    /// <summary>
    /// Gets "today", "tomorrow", "in X days", or a date label for booking display
    /// </summary>
    public static string GetRelativeTime(DateTime time)
    {
        var now = DateTime.Now;
        var span = time.Date - now.Date;

        if (span.TotalDays == 0) return "today";
        if (span.TotalDays == 1) return "tomorrow";
        if (span.TotalDays < 7) return $"in {(int)span.TotalDays} days";

        return time.ToString("d MMM");
    }

    /// <summary>
    /// Load profile page
    /// </summary>
    public async Task<IActionResult> OnGetAsync()
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        var userRole = HttpContext.Session.GetString("UserRole");

        if (userId == null)
            return RedirectToPage("/Login");

        if (userRole != "Student")
            return RedirectToPage("/Index");

        CurrentUserId = userId;
        await LoadAllUserData(userId.Value);
        return Page();
    }

    /// <summary>
    /// Handles password change submission
    /// </summary>
    public async Task<IActionResult> OnPostAsync()
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null)
            return RedirectToPage("/Login");

        var user = await _context.User.FirstOrDefaultAsync(u => u.UserId == userId);
        if (user == null)
        {
            PasswordChangeMessage = "User not found. Weird.";
            return await ReloadAndReturn(userId.Value);
        }

        // ⚠ Validate all fields are filled
        if (string.IsNullOrWhiteSpace(Input.CurrentPassword) ||
            string.IsNullOrWhiteSpace(Input.NewPassword) ||
            string.IsNullOrWhiteSpace(Input.ConfirmPassword))
        {
            PasswordChangeMessage = "Please fill in all the fields.";
            return await ReloadAndReturn(userId.Value);
        }

        // ❌ Check if current password is wrong
        if (Input.CurrentPassword != user.Password)
        {
            PasswordChangeMessage = "Your current password is incorrect.";
            return await ReloadAndReturn(userId.Value);
        }

        // ❌ Don't allow new password to be same
        if (Input.CurrentPassword == Input.NewPassword)
        {
            PasswordChangeMessage = "New password can't be the same as the current one.";
            return await ReloadAndReturn(userId.Value);
        }

        // ❌ Confirm doesn't match
        if (Input.NewPassword != Input.ConfirmPassword)
        {
            PasswordChangeMessage = "New passwords don't match.";
            return await ReloadAndReturn(userId.Value);
        }

        // ❌ Password too short
        if (Input.NewPassword.Length < 6)
        {
            PasswordChangeMessage = "Password should be at least 6 characters.";
            return await ReloadAndReturn(userId.Value);
        }

        // ✅ Save new password
        user.Password = Input.NewPassword;
        await _context.SaveChangesAsync();

        PasswordChangeSuccess = true;
        PasswordChangeMessage = "Password updated successfully!";
        return await OnGetAsync();
    }

    /// <summary>
    /// Handles profile picture URL submission
    /// </summary>
    public async Task<IActionResult> OnPostSetProfilePictureUrlAsync()
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null)
            return RedirectToPage("/Login");

        var user = await _context.User.FirstOrDefaultAsync(u => u.UserId == userId);
        if (user == null)
            return RedirectToPage("/Login");

        // 📛 Empty input
        if (string.IsNullOrWhiteSpace(NewProfileImageUrl))
        {
            ProfilePictureMessage = "Please provide a valid image URL.";
            ProfilePictureSuccess = false;
            return await ReloadAndReturn(userId.Value);
        }

        // 🧼 Validate file extension
        var pattern = @"^https?:\/\/.*\.(jpg|jpeg|png|gif|webp|bmp|svg)$";
        if (!Regex.IsMatch(NewProfileImageUrl, pattern, RegexOptions.IgnoreCase))
        {
            ProfilePictureMessage = "Invalid image URL. Supported formats: .jpg, .jpeg, .png, .gif, .webp, .bmp, .svg";
            ProfilePictureSuccess = false;
            return await ReloadAndReturn(userId.Value);
        }

        // ✅ Save picture
        user.ProfileImagePath = NewProfileImageUrl;
        await _context.SaveChangesAsync();

        ProfilePictureMessage = "Profile picture updated!";
        ProfilePictureSuccess = true;

        return await OnGetAsync();
    }

    /// <summary>
    /// Central method to reload user data
    /// </summary>
    private async Task<IActionResult> ReloadAndReturn(int userId)
    {
        await LoadAllUserData(userId);
        return Page();
    }

    /// <summary>
    /// Loads all required info for the page
    /// </summary>
    private async Task LoadAllUserData(int userId)
    {
        CurrentUserId = userId;
        await LoadUserInfo(userId);
        await LoadUserGroups(userId);
        await LoadUpcomingBookings(userId);
    }

    /// <summary>
    /// Loads name, email, and image path
    /// </summary>
    private async Task LoadUserInfo(int userId)
    {
        var user = await _context.User.FirstOrDefaultAsync(u => u.UserId == userId);
        if (user != null)
        {
            UserName = $"{user.FirstName} {user.LastName}";
            UserEmail = user.Email;
            ProfileImagePath = user.ProfileImagePath;
        }
    }

    /// <summary>
    /// Loads groups that this user belongs to
    /// </summary>
    private async Task LoadUserGroups(int userId)
    {
        UserGroups = await _context.UserGroup
            .Where(ug => ug.UserId == userId)
            .Include(ug => ug.Group)
            .Select(ug => ug.Group)
            .ToListAsync();
    }

    /// <summary>
    /// Loads bookings made by or for user's groups (future ones only)
    /// </summary>
    private async Task LoadUpcomingBookings(int userId)
    {
        var now = DateTime.Now;
        var groupIds = UserGroups.Select(g => g.GroupId).ToHashSet();

        var bookings = await _context.Booking
            .Include(b => b.Room)
            .Include(b => b.Group)
            .Where(b => b.EndTime > now)
            .ToListAsync();

        UpcomingBookings = [.. bookings
            .Where(b => b.CreatedByUserId == userId || groupIds.Contains(b.GroupId))
            .OrderBy(b => b.StartTime)];
    }
}