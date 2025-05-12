using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using DM2Projekt.Data;
using DM2Projekt.Models;
using DM2Projekt.Models.Enums; // <- needed for Role and RoomType enums

namespace DM2Projekt.Pages;

public class IndexModel : PageModel
{
    private readonly DM2ProjektContext _context;

    public IndexModel(DM2ProjektContext context)
    {
        _context = context;
    }

    // Student: their next booking
    public Booking? NextBooking { get; set; }

    // Teacher: how many bookings they can cancel
    public int CancellableBookingCount { get; set; }

    // Teacher: profile image
    public string? TeacherProfileImagePath { get; set; }

    // Admin: user breakdown
    public int UserCount { get; set; }
    public int StudentCount { get; set; }
    public int TeacherCount { get; set; }
    public int AdminCount { get; set; }

    // Admin: room breakdown
    public int RoomCount { get; set; }
    public int ClassroomCount { get; set; }
    public int MeetingRoomCount { get; set; }

    // Admin: group total
    public int GroupCount { get; set; }

    // Admin: booking breakdown
    public int BookingCount { get; set; }
    public int UpcomingBookingCount { get; set; }
    public int PastBookingCount { get; set; }

    public async Task OnGetAsync()
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        var role = HttpContext.Session.GetString("UserRole");
        var now = DateTime.Now;

        // student: get next upcoming booking
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

        // teacher: get how many future bookings they can cancel
        if (role == "Teacher" && userId != null)
        {
            var cutoff = now.AddDays(3);
            CancellableBookingCount = await _context.Booking
                .CountAsync(b => b.StartTime > cutoff);

            var teacher = await _context.User.FirstOrDefaultAsync(u => u.UserId == userId);
            TeacherProfileImagePath = teacher?.ProfileImagePath;
        }

        // admin: full dashboard data
        if (role == "Admin")
        {
            // users by role
            UserCount = await _context.User.CountAsync();
            StudentCount = await _context.User.CountAsync(u => u.Role == Role.Student);
            TeacherCount = await _context.User.CountAsync(u => u.Role == Role.Teacher);
            AdminCount = await _context.User.CountAsync(u => u.Role == Role.Admin);

            // rooms by type
            RoomCount = await _context.Room.CountAsync();
            ClassroomCount = await _context.Room.CountAsync(r => r.RoomType == RoomType.Classroom);
            MeetingRoomCount = await _context.Room.CountAsync(r => r.RoomType == RoomType.MeetingRoom);

            // groups and bookings
            GroupCount = await _context.Group.CountAsync();
            BookingCount = await _context.Booking.CountAsync();
            UpcomingBookingCount = await _context.Booking.CountAsync(b => b.StartTime > now);
            PastBookingCount = await _context.Booking.CountAsync(b => b.EndTime < now);
        }
    }

    // turns a datetime into something like "Starts in 1 hour and 20 minutes"
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
