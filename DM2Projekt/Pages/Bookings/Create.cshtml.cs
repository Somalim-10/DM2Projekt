using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using DM2Projekt.Data;
using DM2Projekt.Models;
using DM2Projekt.Models.Enums;

namespace DM2Projekt.Pages.Bookings
{
    public class CreateModel : PageModel
    {
        private readonly DM2ProjektContext _context;

        public CreateModel(DM2ProjektContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Booking Booking { get; set; } = default!;

        [BindProperty]
        public string SelectedTimeSlot { get; set; } = default!;

        public IActionResult OnGet()
        {
            PopulateDropdowns();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!TryParseSelectedTimeSlot(out var startTime))
                return Page();

            Booking.StartTime = startTime;
            Booking.EndTime = startTime.AddHours(2);

            if (UserHasBookingAtSameTime() || BookingExceedsMaxLength() || GroupHasActiveBooking())
            {
                PopulateDropdowns();
                return Page();
            }

            _context.Booking.Add(Booking);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }

        public JsonResult OnGetSmartboardsByRoom(int roomId)
        {
            var smartboards = _context.Smartboard
                .Where(sb => sb.RoomId == roomId && sb.IsAvailable)
                .Select(sb => new
                {
                    sb.SmartboardId,
                    Display = $"Smartboard {sb.SmartboardId}"
                })
                .ToList();

            return new JsonResult(smartboards);
        }

        public JsonResult OnGetAvailableTimeSlots(int roomId, DateTime date)
        {
            var room = _context.Room.FirstOrDefault(r => r.RoomId == roomId);
            if (room == null)
                return new JsonResult(new { error = "Room not found" });

            var slots = GetFixedTimeSlots(date);
            var bookings = _context.Booking
                .Where(b => b.RoomId == roomId && b.StartTime.Date == date.Date)
                .ToList();

            var availableSlots = slots
                .Where(slot => IsSlotAvailable(room, bookings, slot))
                .Select(slot => new
                {
                    start = slot.start.ToString("HH:mm"),
                    end = slot.end.ToString("HH:mm"),
                    value = slot.start.ToString("o")
                })
                .ToList();

            return new JsonResult(availableSlots);
        }

        private void PopulateDropdowns()
        {
            ViewData["GroupId"] = new SelectList(_context.Group, "GroupId", "GroupName");
            ViewData["RoomId"] = new SelectList(_context.Room, "RoomId", "RoomName");
            ViewData["SmartboardId"] = new SelectList(_context.Smartboard, "SmartboardId", "SmartboardId");
            ViewData["CreatedByUserId"] = new SelectList(_context.User, "UserId", "Email");
        }

        private bool TryParseSelectedTimeSlot(out DateTime startTime)
        {
            if (!DateTime.TryParse(SelectedTimeSlot, out startTime))
            {
                ModelState.AddModelError("SelectedTimeSlot", "Invalid time slot selected.");
                PopulateDropdowns();
                return false;
            }
            return true;
        }

        private bool UserHasBookingAtSameTime()
        {
            bool conflict = _context.Booking.Any(b =>
                b.CreatedByUserId == Booking.CreatedByUserId &&
                b.StartTime < Booking.EndTime &&
                b.EndTime > Booking.StartTime);

            if (conflict)
                ModelState.AddModelError(string.Empty, "This user already has a booking at the same time.");

            return conflict;
        }

        private bool BookingExceedsMaxLength()
        {
            if ((Booking.EndTime - Booking.StartTime).TotalHours > 2)
            {
                ModelState.AddModelError(string.Empty, "En booking må maksimalt vare 2 timer.");
                return true;
            }
            return false;
        }

        private bool GroupHasActiveBooking()
        {
            bool activeBooking = _context.Booking
                .Any(b => b.GroupId == Booking.GroupId && b.EndTime > DateTime.Now);

            if (activeBooking)
                ModelState.AddModelError(string.Empty, "This group already has an active booking.");

            return activeBooking;
        }

        private static bool IsSlotAvailable(Room room, List<Booking> bookings, (DateTime start, DateTime end) slot)
        {
            var bookingsInSlot = bookings.Count(b =>
                b.StartTime == slot.start && b.EndTime == slot.end);

            return room.RoomType switch
            {
                RoomType.Classroom => bookingsInSlot < 2,
                RoomType.MeetingRoom => bookingsInSlot < 1,
                _ => false
            };
        }

        private static List<(DateTime start, DateTime end)> GetFixedTimeSlots(DateTime day)
        {
            var date = day.Date;
            return new List<(DateTime, DateTime)>
            {
                (date.AddHours(8), date.AddHours(10)),
                (date.AddHours(10), date.AddHours(12)),
                (date.AddHours(12), date.AddHours(14)),
                (date.AddHours(14), date.AddHours(16))
            };
        }
    }
}
