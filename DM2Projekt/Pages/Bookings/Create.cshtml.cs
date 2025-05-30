using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using DM2Projekt.Data;
using DM2Projekt.Models;
using System.ComponentModel.DataAnnotations;

namespace DM2Projekt.Pages.Bookings;

[IgnoreAntiforgeryToken]
public class CreateModel : PageModel
{
    private readonly DM2ProjektContext _context;

    public CreateModel(DM2ProjektContext context)
    {
        _context = context;
    }

    // Booking being created
    [BindProperty]
    public Booking Booking { get; set; } = default!;

    // These come from hidden inputs on the form
    [BindProperty, Required]
    public string SelectedTimeSlot { get; set; } = "";

    [BindProperty, Required]
    public string SelectedWeek { get; set; } = "";

    [BindProperty, Required]
    public string SelectedDay { get; set; } = "";

    public IActionResult OnGet()
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null) return RedirectToPage("/Login");

        var userRole = HttpContext.Session.GetString("UserRole");
        if (userRole == "Teacher") return RedirectToPage("/Bookings/Index");

        PopulateDropdowns();
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        var userRole = HttpContext.Session.GetString("UserRole");

        if (userId == null) return RedirectToPage("/Login");
        if (userRole == "Teacher") return RedirectToPage("/Bookings/Index");

        // Don't validate Start/End from the form (we generate them)
        ModelState.Remove("Booking.StartTime");
        ModelState.Remove("Booking.EndTime");

        if (!ModelState.IsValid)
        {
            PopulateDropdowns();
            return Page();
        }

        if (!TryParseAndValidateTimeSlot(out var startTime))
        {
            PopulateDropdowns();
            return Page();
        }

        Booking.StartTime = startTime;
        Booking.EndTime = startTime.AddHours(2);

        if (userRole == "Student")
            Booking.CreatedByUserId = userId.Value;

        var room = GetRoom(Booking.RoomId);
        if (room?.RoomType == RoomType.MeetingRoom)
            Booking.UsesSmartboard = true;

        if (!ValidateBooking())
        {
            PopulateDropdowns();
            return Page();
        }

        _context.Booking.Add(Booking);
        await _context.SaveChangesAsync();

        return RedirectToPage("./Index");
    }

    // === Dropdown Helpers ===

    private void PopulateDropdowns()
    {
        var role = HttpContext.Session.GetString("UserRole");
        var userId = HttpContext.Session.GetInt32("UserId");

        if (role == "Admin" || role == "Teacher")
        {
            ViewData["GroupId"] = new SelectList(_context.Group, "GroupId", "GroupName");
            ViewData["CreatedByUserId"] = new SelectList(_context.User, "UserId", "Email");
        }
        else if (role == "Student" && userId != null)
        {
            var myGroups = _context.UserGroup
                .Where(ug => ug.UserId == userId)
                .Select(ug => ug.Group)
                .ToList();

            ViewData["GroupId"] = new SelectList(myGroups, "GroupId", "GroupName");
        }

        ViewData["RoomId"] = new SelectList(_context.Room, "RoomId", "RoomName");
    }

    private Room? GetRoom(int roomId) =>
        _context.Room.FirstOrDefault(r => r.RoomId == roomId);

    private List<Booking> GetBookingsForRoomOnDate(int roomId, DateTime date) =>
        _context.Booking
            .Where(b => b.RoomId == roomId && b.StartTime != null && b.StartTime.Value.Date == date.Date)
            .ToList();

    private static List<(DateTime start, DateTime end)> GetFixedTimeSlots(DateTime day)
    {
        var date = day.Date;
        return new()
        {
            (date.AddHours(8), date.AddHours(10)),
            (date.AddHours(10), date.AddHours(12)),
            (date.AddHours(12), date.AddHours(14)),
            (date.AddHours(14), date.AddHours(16))
        };
    }

    private static bool IsSlotAvailable(Room room, List<Booking> bookings, (DateTime start, DateTime end) slot)
    {
        return room.RoomType switch
        {
            RoomType.Classroom => bookings.Count(b => IsSameSlot(b, slot)) < 2,
            RoomType.MeetingRoom => !bookings.Any(b => IsSameSlot(b, slot)),
            _ => false
        };
    }

    private static bool IsSameSlot(Booking b, (DateTime start, DateTime end) slot) =>
        b.StartTime == slot.start && b.EndTime == slot.end;

    private static object FormatSlot((DateTime start, DateTime end) slot) => new
    {
        start = slot.start.ToString("HH:mm"),
        end = slot.end.ToString("HH:mm"),
        value = slot.start.ToString("o")
    };

    private bool TryParseAndValidateTimeSlot(out DateTime startTime)
    {
        if (!DateTime.TryParse(SelectedTimeSlot, out startTime))
        {
            ModelState.AddModelError(string.Empty, "Invalid time slot selected.");
            return false;
        }
        return true;
    }

    // === Booking Validation ===

    private bool ValidateBooking()
    {
        return !CheckIfBookingIsInThePast()
            && !HasUserBookingConflict()
            && !BookingExceedsMaxLength()
            && !GroupAlreadyHasBooking()
            && !IsSmartboardAlreadyInUse()
            && !HasGroupBookingConflict();
    }

    private bool CheckIfBookingIsInThePast()
    {
        if (Booking.StartTime < DateTime.Now)
        {
            ModelState.AddModelError(nameof(Booking.StartTime), "Booking can't be in the past.");
            return true;
        }
        return false;
    }

    private bool HasUserBookingConflict()
    {
        var conflict = _context.Booking.Any(b =>
            b.CreatedByUserId == Booking.CreatedByUserId &&
            b.StartTime < Booking.EndTime &&
            b.EndTime > Booking.StartTime);

        if (conflict)
            ModelState.AddModelError(nameof(Booking.CreatedByUserId), "User already has a booking at this time.");

        return conflict;
    }

    private bool HasGroupBookingConflict()
    {
        var conflict = _context.Booking.Any(b =>
            b.GroupId == Booking.GroupId &&
            b.StartTime < Booking.EndTime &&
            b.EndTime > Booking.StartTime);

        if (conflict)
            ModelState.AddModelError(nameof(Booking.GroupId), "This group already has a booking at this time.");

        return conflict;
    }

    private bool BookingExceedsMaxLength()
    {
        if ((Booking.EndTime - Booking.StartTime)?.TotalHours > 2)
        {
            ModelState.AddModelError(nameof(SelectedTimeSlot), "Booking can maximum last 2 hours.");
            return true;
        }
        return false;
    }

    private bool GroupAlreadyHasBooking()
    {
        int active = _context.Booking
            .Count(b => b.GroupId == Booking.GroupId && b.EndTime > DateTime.Now);

        if (active >= 3)
        {
            ModelState.AddModelError(nameof(Booking.GroupId), "This group already has 3 or more active bookings.");
            return true;
        }

        return false;
    }

    private bool IsSmartboardAlreadyInUse()
    {
        if (!Booking.UsesSmartboard) return false;

        bool taken = _context.Booking.Any(b =>
            b.RoomId == Booking.RoomId &&
            b.StartTime == Booking.StartTime &&
            b.EndTime == Booking.EndTime &&
            b.UsesSmartboard);

        if (taken)
            ModelState.AddModelError(nameof(Booking.UsesSmartboard), "Smartboard already booked at this time.");

        return taken;
    }

    // === AJAX ===

    public JsonResult OnGetAvailableTimeSlots(int roomId, DateTime date)
    {
        var room = GetRoom(roomId);
        if (room == null)
            return new JsonResult(new { error = "Room not found" });

        var slots = GetFixedTimeSlots(date);
        var bookings = GetBookingsForRoomOnDate(roomId, date);

        var available = slots
            .Where(slot => IsSlotAvailable(room, bookings, slot))
            .Select(FormatSlot)
            .ToList();

        return new JsonResult(available);
    }

    public JsonResult OnGetRoomType(int roomId)
    {
        var room = GetRoom(roomId);
        if (room == null)
            return new JsonResult(new { error = "Room not found" });

        return new JsonResult(new { roomType = room.RoomType.ToString() });
    }

    public JsonResult OnGetSmartboardCheck(int roomId, DateTime start, DateTime end)
    {
        var room = GetRoom(roomId);
        if (room?.RoomType != RoomType.Classroom)
            return new JsonResult(false);

        var taken = _context.Booking
            .Where(b => b.RoomId == roomId && b.UsesSmartboard && b.StartTime.HasValue && b.EndTime.HasValue)
            .ToList()
            .Any(b => AreTimesEqualToMinute(b.StartTime.Value, start) && AreTimesEqualToMinute(b.EndTime.Value, end));

        return new JsonResult(taken);
    }

    // Without this we get a mismatch between how data is stored in JS vs C#
    private bool AreTimesEqualToMinute(DateTime a, DateTime b) =>
        a.ToUniversalTime().ToString("yyyy-MM-dd HH:mm") ==
        b.ToUniversalTime().ToString("yyyy-MM-dd HH:mm");
}