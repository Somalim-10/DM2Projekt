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

    // What students see
    public Booking? NextBooking { get; set; }

    // What teacher sees
    public int CancellableBookingCount { get; set; }
    public string? TeacherProfileImagePath { get; set; }

    // What admin sees about users
    public int UserCount { get; set; }
    public int StudentCount { get; set; }
    public int TeacherCount { get; set; }
    public int AdminCount { get; set; }

    // What admin sees about rooms
    public int RoomCount { get; set; }
    public int ClassroomCount { get; set; }
    public int MeetingRoomCount { get; set; }
    public string? MostUsedRoomName { get; set; }

    // What admin sees about groups
    public int GroupCount { get; set; }

    // What admin sees about bookings
    public int BookingCount { get; set; }
    public int UpcomingBookingCount { get; set; }
    public int PastBookingCount { get; set; }
    public int OngoingBookingCount { get; set; }
    public int StartingSoonBookingCount { get; set; }

    public async Task OnGetAsync()
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        var role = HttpContext.Session.GetString("UserRole");
        var now = DateTime.Now;
        var soon = now.AddHours(1);

        if (role == "Student" && userId != null)
            await LoadStudentData(userId.Value, now);

        if (role == "Teacher" && userId != null)
            await LoadTeacherData(userId.Value, now);

        if (role == "Admin")
            await LoadAdminData(now, soon);
    }

    private async Task LoadStudentData(int userId, DateTime now)
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

        // find booking for user or their groups
        NextBooking = upcoming.FirstOrDefault(b =>
            b.CreatedByUserId == userId || groupIds.Contains(b.GroupId));
    }

    private async Task LoadTeacherData(int userId, DateTime now)
    {
        var cutoff = now.AddDays(3);

        CancellableBookingCount = await _context.Booking
            .CountAsync(b => b.StartTime > cutoff);

        TeacherProfileImagePath = await _context.User
            .Where(u => u.UserId == userId)
            .Select(u => u.ProfileImagePath)
            .FirstOrDefaultAsync();
    }

    private async Task LoadAdminData(DateTime now, DateTime soon)
    {
        // Users
        UserCount = await _context.User.CountAsync();
        StudentCount = await _context.User.CountAsync(u => u.Role == Role.Student);
        TeacherCount = await _context.User.CountAsync(u => u.Role == Role.Teacher);
        AdminCount = await _context.User.CountAsync(u => u.Role == Role.Admin);

        // Rooms
        RoomCount = await _context.Room.CountAsync();
        ClassroomCount = await _context.Room.CountAsync(r => r.RoomType == RoomType.Classroom);
        MeetingRoomCount = await _context.Room.CountAsync(r => r.RoomType == RoomType.MeetingRoom);

        MostUsedRoomName = await _context.Booking
            .GroupBy(b => b.Room.RoomName)
            .OrderByDescending(g => g.Count())
            .Select(g => g.Key)
            .FirstOrDefaultAsync();

        // Groups
        GroupCount = await _context.Group.CountAsync();

        // Bookings
        BookingCount = await _context.Booking.CountAsync();
        UpcomingBookingCount = await _context.Booking.CountAsync(b => b.StartTime > now);
        PastBookingCount = await _context.Booking.CountAsync(b => b.EndTime < now);
        OngoingBookingCount = await _context.Booking.CountAsync(b => b.StartTime <= now && b.EndTime > now);
        StartingSoonBookingCount = await _context.Booking.CountAsync(b => b.StartTime > now && b.StartTime <= soon);
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
