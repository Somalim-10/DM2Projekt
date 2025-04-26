using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using DM2Projekt.Data;
using DM2Projekt.Models;
using DM2Projekt.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace DM2Projekt.Pages.Bookings;

// backend for create booking page
public class CreateModel : PageModel
{
    private readonly DM2ProjektContext _context;

    public CreateModel(DM2ProjektContext context)
    {
        _context = context;
    }

    // booking to save
    [BindProperty]
    public Booking Booking { get; set; } = default!;

    // hidden fields for week, day and time slot
    [BindProperty]
    [Required]
    public string SelectedTimeSlot { get; set; } = "";

    [BindProperty]
    [Required]
    public string SelectedWeek { get; set; } = "";

    [BindProperty]
    [Required]
    public string SelectedDay { get; set; } = "";

    // when page loads
    public IActionResult OnGet()
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null) // if not logged in
            return RedirectToPage("/Login");

        PopulateDropdowns();
        return Page();
    }

    // when form is submitted
    public async Task<IActionResult> OnPostAsync()
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null)
            return RedirectToPage("/Login");

        // skip validation on StartTime and EndTime (we set manually)
        ModelState.Remove("Booking.StartTime");
        ModelState.Remove("Booking.EndTime");

        // check other fields
        if (!ModelState.IsValid)
        {
            PopulateDropdowns();
            return Page();
        }

        // try to parse selected timeslot
        if (!TryParseAndValidateTimeSlot(out var startTime))
        {
            PopulateDropdowns();
            return Page();
        }

        // set booking start and end time (always 2 hours)
        Booking.StartTime = startTime;
        Booking.EndTime = startTime.AddHours(2);

        if (!ModelState.IsValid)
        {
            PopulateDropdowns();
            return Page();
        }

        // if meeting room, always auto-enable smartboard
        var room = GetRoom(Booking.RoomId);
        if (room?.RoomType == RoomType.MeetingRoom)
            Booking.UsesSmartboard = true;

        // validate booking (conflicts, etc.)
        if (!ValidateBooking())
        {
            PopulateDropdowns();
            return Page();
        }

        // force CreatedByUserId for students
        var userRole = HttpContext.Session.GetString("UserRole");
        if (userRole == "Student")
        {
            Booking.CreatedByUserId = userId.Value;
        }

        // save to database
        _context.Booking.Add(Booking);
        await _context.SaveChangesAsync();

        return RedirectToPage("./Index");
    }

    // --- AJAX handlers below ---

    // get available time slots for a room and date
    public JsonResult OnGetAvailableTimeSlots(int roomId, DateTime date)
    {
        var room = GetRoom(roomId);
        if (room == null)
            return new JsonResult(new { error = "Room not found" });

        var slots = GetFixedTimeSlots(date);
        var bookings = GetBookingsForRoomOnDate(roomId, date);

        var availableSlots = slots
            .Where(slot => IsSlotAvailable(room, bookings, slot))
            .Select(FormatSlot)
            .ToList();

        return new JsonResult(availableSlots);
    }

    // get room type (classroom or meeting room)
    public JsonResult OnGetRoomType(int roomId)
    {
        var room = GetRoom(roomId);
        if (room == null)
            return new JsonResult(new { error = "Room not found" });

        return new JsonResult(new { roomType = room.RoomType.ToString() });
    }

    // check if smartboard is available
    public JsonResult OnGetSmartboardAvailability(int roomId, DateTime startTime, DateTime endTime)
    {
        var room = GetRoom(roomId);
        if (room?.RoomType != RoomType.Classroom)
            return new JsonResult(new { available = false });

        bool isTaken = _context.Booking.Any(b =>
            b.RoomId == roomId &&
            b.StartTime == startTime &&
            b.EndTime == endTime &&
            b.UsesSmartboard);

        return new JsonResult(new { available = !isTaken });
    }

    // check if smartboard already booked
    public JsonResult OnGetSmartboardCheck(int roomId, DateTime start, DateTime end)
    {
        var room = GetRoom(roomId);
        if (room?.RoomType != RoomType.Classroom)
            return new JsonResult(false);

        bool smartboardUsed = _context.Booking.Any(b =>
            b.RoomId == roomId &&
            b.StartTime == start &&
            b.EndTime == end &&
            b.UsesSmartboard);

        return new JsonResult(smartboardUsed);
    }

    // --- Helper methods below ---

    // fill dropdown lists
    private void PopulateDropdowns()
    {
        ViewData["GroupId"] = new SelectList(_context.Group, "GroupId", "GroupName");
        ViewData["RoomId"] = new SelectList(_context.Room, "RoomId", "RoomName");

        var userRole = HttpContext.Session.GetString("UserRole");
        if (userRole == "Admin" || userRole == "Teacher")
        {
            ViewData["CreatedByUserId"] = new SelectList(_context.User, "UserId", "Email");
        }
    }

    // get room by ID
    private Room? GetRoom(int roomId) =>
        _context.Room.FirstOrDefault(r => r.RoomId == roomId);

    // get bookings for a room on a specific date
    private List<Booking> GetBookingsForRoomOnDate(int roomId, DateTime date) =>
        [.. _context.Booking
            .Where(b => b.RoomId == roomId && b.StartTime != null && b.StartTime.Value.Date == date.Date)];

    // fixed time slots every day
    private static List<(DateTime start, DateTime end)> GetFixedTimeSlots(DateTime day)
    {
        var date = day.Date;
        return
        [
            (date.AddHours(8), date.AddHours(10)),
            (date.AddHours(10), date.AddHours(12)),
            (date.AddHours(12), date.AddHours(14)),
            (date.AddHours(14), date.AddHours(16))
        ];
    }

    // check if a time slot is available
    private static bool IsSlotAvailable(Room room, List<Booking> bookings, (DateTime start, DateTime end) slot)
    {
        return room.RoomType switch
        {
            RoomType.Classroom => bookings.Count(b => IsSameSlot(b, slot)) < 2,
            RoomType.MeetingRoom => !bookings.Any(b => IsSameSlot(b, slot)),
            _ => false
        };
    }

    // check if two slots match
    private static bool IsSameSlot(Booking booking, (DateTime start, DateTime end) slot) =>
        booking.StartTime == slot.start && booking.EndTime == slot.end;

    // format slot for JSON
    private static object FormatSlot((DateTime start, DateTime end) slot) => new
    {
        start = slot.start.ToString("HH:mm"),
        end = slot.end.ToString("HH:mm"),
        value = slot.start.ToString("o")
    };

    // try parsing the selected timeslot
    private bool TryParseAndValidateTimeSlot(out DateTime startTime)
    {
        if (!DateTime.TryParse(SelectedTimeSlot, out startTime))
        {
            ModelState.AddModelError(string.Empty, "Invalid time slot selected.");
            return false;
        }
        return true;
    }

    // validate entire booking
    private bool ValidateBooking()
    {
        if (HasUserBookingConflict()) return false;
        if (BookingExceedsMaxLength()) return false;
        if (GroupAlreadyHasBooking()) return false;
        if (IsSmartboardAlreadyInUse()) return false;

        return true;
    }

    // user cannot book two bookings at once
    private bool HasUserBookingConflict()
    {
        bool conflict = _context.Booking.Any(b =>
            b.CreatedByUserId == Booking.CreatedByUserId &&
            b.StartTime < Booking.EndTime &&
            b.EndTime > Booking.StartTime);

        if (conflict)
            ModelState.AddModelError(nameof(Booking.CreatedByUserId), "User already has a booking at this time.");

        return conflict;
    }

    // booking cannot last longer than 2 hours
    private bool BookingExceedsMaxLength()
    {
        if (Booking.StartTime != null && Booking.EndTime != null &&
            (Booking.EndTime.Value - Booking.StartTime.Value).TotalHours > 2)
        {
            ModelState.AddModelError(nameof(SelectedTimeSlot), "Booking can maximum last 2 hours.");
            return true;
        }
        return false;
    }

    // a group can only have one active booking
    private bool GroupAlreadyHasBooking()
    {
        bool activeBooking = _context.Booking
            .Any(b => b.GroupId == Booking.GroupId && b.EndTime > DateTime.Now);

        if (activeBooking)
            ModelState.AddModelError(nameof(Booking.GroupId), "This group already has an active booking.");

        return activeBooking;
    }

    // smartboard must not be double-booked
    private bool IsSmartboardAlreadyInUse()
    {
        if (!Booking.UsesSmartboard)
            return false;

        bool smartboardUsed = _context.Booking.Any(b =>
            b.RoomId == Booking.RoomId &&
            b.StartTime == Booking.StartTime &&
            b.EndTime == Booking.EndTime &&
            b.UsesSmartboard);

        if (smartboardUsed)
            ModelState.AddModelError(nameof(Booking.UsesSmartboard), "Smartboard already booked at this time.");

        return smartboardUsed;
    }
}
