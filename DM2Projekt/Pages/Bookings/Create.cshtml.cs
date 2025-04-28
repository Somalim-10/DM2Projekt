using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using DM2Projekt.Data;
using DM2Projekt.Models;
using DM2Projekt.Models.Enums;
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

    // booking info that we save
    [BindProperty]
    public Booking Booking { get; set; } = default!;

    // hidden fields from form
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

        var userRole = HttpContext.Session.GetString("UserRole");
        if (userRole == "Teacher") // teachers shouldn't book
        {
            return RedirectToPage("/Bookings/Index");
        }

        PopulateDropdowns();
        return Page();
    }

    // when form is submitted
    public async Task<IActionResult> OnPostAsync()
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        var userRole = HttpContext.Session.GetString("UserRole");

        if (userId == null) // if not logged in
            return RedirectToPage("/Login");

        if (userRole == "Teacher") // teachers shouldn't book
        {
            return RedirectToPage("/Bookings/Index");
        }

        ModelState.Remove("Booking.StartTime");
        ModelState.Remove("Booking.EndTime");

        if (!ModelState.IsValid)
        {
            PopulateDropdowns();
            return Page();
        }

        // parse selected timeslot
        if (!TryParseAndValidateTimeSlot(out var startTime))
        {
            PopulateDropdowns();
            return Page();
        }

        // set booking start and end (always 2 hours)
        Booking.StartTime = startTime;
        Booking.EndTime = startTime.AddHours(2);

        // for students, force their own userId
        if (userRole == "Student")
        {
            Booking.CreatedByUserId = userId.Value;
        }

        var room = GetRoom(Booking.RoomId);
        // auto-enable smartboard if meeting room
        if (room?.RoomType == RoomType.MeetingRoom)
            Booking.UsesSmartboard = true;

        // extra validation
        if (!ValidateBooking())
        {
            PopulateDropdowns();
            return Page();
        }

        // save booking
        _context.Booking.Add(Booking);
        await _context.SaveChangesAsync();

        return RedirectToPage("./Index");
    }

    // load dropdown lists
    private void PopulateDropdowns()
    {
        var userRole = HttpContext.Session.GetString("UserRole");
        var userId = HttpContext.Session.GetInt32("UserId");

        if (userRole == "Admin" || userRole == "Teacher")
        {
            ViewData["GroupId"] = new SelectList(_context.Group, "GroupId", "GroupName");
            ViewData["CreatedByUserId"] = new SelectList(_context.User, "UserId", "Email");
        }
        else if (userRole == "Student" && userId != null)
        {
            var myGroups = _context.UserGroup
                .Where(ug => ug.UserId == userId)
                .Select(ug => ug.Group)
                .ToList();

            ViewData["GroupId"] = new SelectList(myGroups, "GroupId", "GroupName");
        }

        ViewData["RoomId"] = new SelectList(_context.Room, "RoomId", "RoomName");
    }

    // get a room from DB
    private Room? GetRoom(int roomId) =>
        _context.Room.FirstOrDefault(r => r.RoomId == roomId);

    // get bookings for a specific day
    private List<Booking> GetBookingsForRoomOnDate(int roomId, DateTime date) =>
        [.. _context.Booking
            .Where(b => b.RoomId == roomId && b.StartTime != null && b.StartTime.Value.Date == date.Date)];

    // generate fixed time slots (always same times)
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

    // format slot for frontend (start, end, value)
    private static object FormatSlot((DateTime start, DateTime end) slot) => new
    {
        start = slot.start.ToString("HH:mm"),
        end = slot.end.ToString("HH:mm"),
        value = slot.start.ToString("o")
    };

    // parse and validate the selected slot
    private bool TryParseAndValidateTimeSlot(out DateTime startTime)
    {
        if (!DateTime.TryParse(SelectedTimeSlot, out startTime))
        {
            ModelState.AddModelError(string.Empty, "Invalid time slot selected.");
            return false;
        }
        return true;
    }

    // validate the booking
    private bool ValidateBooking()
    {
        if (HasUserBookingConflict()) return false;
        if (BookingExceedsMaxLength()) return false;
        if (GroupAlreadyHasBooking()) return false;
        if (IsSmartboardAlreadyInUse()) return false;
        if (HasGroupBookingConflict()) return false;
        return true;
    }

    //check if user has another booking at the same time
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

    // Check if group already has another booking at the same time
    private bool HasGroupBookingConflict()
    {
        bool conflict = _context.Booking.Any(b =>
            b.GroupId == Booking.GroupId &&
            b.StartTime < Booking.EndTime &&
            b.EndTime > Booking.StartTime);

        if (conflict)
            ModelState.AddModelError(nameof(Booking.GroupId), "This group already has a booking at this time.");

        return conflict;
    }

    // check if booking is longer than 2 hours
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

    // Check if group already has 3 or more active bookings
    private bool GroupAlreadyHasBooking()
    {
        int activeBookingCount = _context.Booking
            .Count(b => b.GroupId == Booking.GroupId && b.EndTime > DateTime.Now);

        if (activeBookingCount >= 3)
        {
            ModelState.AddModelError(nameof(Booking.GroupId), "This group already has 3 or more active bookings.");
            return true;
        }

        return false;
    }

    // check if smartboard is already taken
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

    // --- AJAX handlers ---

    // send available time slots for selected room/date
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

    // send room type info (Classroom/MeetingRoom)
    public JsonResult OnGetRoomType(int roomId)
    {
        var room = GetRoom(roomId);
        if (room == null)
            return new JsonResult(new { error = "Room not found" });

        return new JsonResult(new { roomType = room.RoomType.ToString() });
    }

    // check if smartboard is already booked
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
}
