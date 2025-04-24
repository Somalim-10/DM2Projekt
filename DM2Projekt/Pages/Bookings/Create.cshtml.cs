using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using DM2Projekt.Data;
using DM2Projekt.Models;

namespace DM2Projekt.Pages.Bookings
{
    public class CreateModel : PageModel
    {
        private readonly DM2Projekt.Data.DM2ProjektContext _context;

        public CreateModel(DM2Projekt.Data.DM2ProjektContext context)
        {
            _context = context;
        }

        public IActionResult OnGet()
        {
            ViewData["GroupId"] = new SelectList(_context.Group, "GroupId", "GroupName");
            ViewData["RoomId"] = new SelectList(_context.Room, "RoomId", "RoomName");
            ViewData["SmartboardId"] = new SelectList(_context.Smartboard, "SmartboardId", "SmartboardId");
            ViewData["CreatedByUserId"] = new SelectList(_context.User, "UserId", "Email");

            return Page();
        }

        [BindProperty]
        public Booking Booking { get; set; } = default!;

        // For more information, see https://aka.ms/RazorPagesCRUD.
        public async Task<IActionResult> OnPostAsync()
        {
            //if (!ModelState.IsValid)
            //{
            //    return Page();
            //}

            ViewData["GroupId"] = new SelectList(_context.Group, "GroupId", "GroupName");
            ViewData["RoomId"] = new SelectList(_context.Room, "RoomId", "RoomName");
            ViewData["SmartboardId"] = new SelectList(_context.Smartboard, "SmartboardId", "SmartboardId");
            ViewData["CreatedByUserId"] = new SelectList(_context.User, "UserId", "Email");

            // Begræns booking til max 2 timer
            TimeSpan bookingLength = Booking.EndTime - Booking.StartTime;
            if (bookingLength.TotalHours > 2)
            {
                ModelState.AddModelError(string.Empty, "En booking må maksimalt vare 2 timer.");
                return Page();
            }

            _context.Booking.Add(Booking);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }

        public JsonResult OnGetSmartboardsByRoom(int roomId)
        {
            var smartboards = _context.Smartboard
                .Where(sb => sb.RoomId == roomId && sb.IsAvailable == true)
                .Select(sb => new
                {
                    sb.SmartboardId,
                    Display = $"Smartboard {sb.SmartboardId}"
                })
                .ToList();

            return new JsonResult(smartboards);
        }
    }
}
