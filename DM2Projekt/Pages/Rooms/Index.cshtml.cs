using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using DM2Projekt.Data;
using DM2Projekt.Models;
using DM2Projekt.Models.Enums;

namespace DM2Projekt.Pages.Rooms
{
    public class IndexModel : PageModel
    {
        private readonly DM2ProjektContext _context;
        public IndexModel(DM2ProjektContext context)
            => _context = context;  // yo: DI our DB context

        // 🔍 free-text search
        [BindProperty(SupportsGet = true)]
        public string? SearchTerm { get; set; }

        // 🏠📶🏷 filter enums
        [BindProperty(SupportsGet = true)]
        public Building? Building { get; set; }
        [BindProperty(SupportsGet = true)]
        public Floor? Floor { get; set; }
        [BindProperty(SupportsGet = true)]
        public RoomType? RoomType { get; set; }

        // ⏰ quick “available next X hours”
        [BindProperty(SupportsGet = true)]
        public bool ShowAvailable { get; set; }
        [BindProperty(SupportsGet = true)]
        public int AvailableHours { get; set; } = 2;

        // dropdown data always from full table
        public List<SelectListItem> BuildingOptions { get; set; } = new();
        public List<SelectListItem> FloorOptions { get; set; } = new();
        public List<SelectListItem> RoomTypeOptions { get; set; } = new();

        // final list we render in the UI
        public List<Room> Rooms { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            // 🚧 require login
            if (HttpContext.Session.GetInt32("UserId") == null)
                return RedirectToPage("/Login");

            // 0) load dropdowns from ALL rooms, so you always see every building/floor/type combo
            var allRooms = await _context.Room.ToListAsync();
            BuildingOptions = allRooms
                .Select(r => r.Building)
                .Distinct()
                .OrderBy(b => (int)b)
                .Select(b => new SelectListItem(b.ToString(), ((int)b).ToString()))
                .ToList();
            FloorOptions = allRooms
                .Select(r => r.Floor)
                .Distinct()
                .OrderBy(f => (int)f)
                .Select(f => new SelectListItem(f.ToString(), ((int)f).ToString()))
                .ToList();
            RoomTypeOptions = allRooms
                .Select(r => r.RoomType)
                .Distinct()
                .OrderBy(rt => (int)rt)
                .Select(rt => new SelectListItem(rt.ToString(), ((int)rt).ToString()))
                .ToList();

            // 1) apply building/floor/type filters via SQL
            var query = _context.Room.AsQueryable();
            if (Building.HasValue) query = query.Where(r => r.Building == Building.Value);
            if (Floor.HasValue) query = query.Where(r => r.Floor == Floor.Value);
            if (RoomType.HasValue) query = query.Where(r => r.RoomType == RoomType.Value);

            // 2) pull into memory for further C# filtering
            var list = await query.ToListAsync();

            // 3) free-text search (in C# to avoid SQL hiccups)
            if (!string.IsNullOrWhiteSpace(SearchTerm))
            {
                var st = SearchTerm.Trim();
                list = list
                    .Where(r => r.RoomName.IndexOf(st, StringComparison.OrdinalIgnoreCase) >= 0)
                    .ToList();
            }

            // 4) availability filter (next X hours) also in C#
            if (ShowAvailable)
            {
                var now = DateTime.Now;
                var cutoff = now.AddHours(AvailableHours);
                var busyIds = await _context.Booking
                    .Where(b => b.StartTime < cutoff && b.EndTime > now)
                    .Select(b => b.RoomId)
                    .Distinct()
                    .ToListAsync();
                list = list.Where(r => !busyIds.Contains(r.RoomId)).ToList();
            }

            Rooms = list;
            return Page();
        }
    }
}
