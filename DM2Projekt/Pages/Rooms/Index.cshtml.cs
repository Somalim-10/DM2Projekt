using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using DM2Projekt.Data;
using DM2Projekt.Models;
using DM2Projekt.Models.Enums;

namespace DM2Projekt.Pages.Rooms;

public class IndexModel : PageModel
{
    private readonly DM2ProjektContext _context;
    public IndexModel(DM2ProjektContext context)
        => _context = context;  // yo: DI our DB context

    // 🔍 search + filters from query
    [BindProperty(SupportsGet = true)] public string? SearchTerm { get; set; }
    [BindProperty(SupportsGet = true)] public Building? Building { get; set; }
    [BindProperty(SupportsGet = true)] public Floor? Floor { get; set; }
    [BindProperty(SupportsGet = true)] public RoomType? RoomType { get; set; }

    // dropdown options (populated every time)
    public List<SelectListItem> BuildingOptions { get; set; } = new();
    public List<SelectListItem> FloorOptions { get; set; } = new();
    public List<SelectListItem> RoomTypeOptions { get; set; } = new();

    // final rooms to render
    public List<Room> Rooms { get; set; } = new();

    public async Task<IActionResult> OnGetAsync()
    {
        // 👮‍♂️ block unauthenticated users
        if (HttpContext.Session.GetInt32("UserId") == null)
            return RedirectToPage("/Login");

        // fill dropdowns with unique values
        await LoadDropdownOptionsAsync();

        // fetch + filter rooms
        Rooms = await GetFilteredRoomsAsync();

        return Page();
    }

    // 🧙 get dropdowns from all rooms (nothing fancy, just unique enums)
    private async Task LoadDropdownOptionsAsync()
    {
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
    }

    // 📦 fetch + filter all the rooms for display
    private async Task<List<Room>> GetFilteredRoomsAsync()
    {
        var query = _context.Room.AsQueryable();

        // apply filters (building/floor/type) if any
        if (Building.HasValue)
            query = query.Where(r => r.Building == Building.Value);

        if (Floor.HasValue)
            query = query.Where(r => r.Floor == Floor.Value);

        if (RoomType.HasValue)
            query = query.Where(r => r.RoomType == RoomType.Value);

        var list = await query.ToListAsync();

        // text search (case-insensitive, C# side)
        if (!string.IsNullOrWhiteSpace(SearchTerm))
        {
            var st = SearchTerm.Trim();
            list = list
                .Where(r => r.RoomName.IndexOf(st, StringComparison.OrdinalIgnoreCase) >= 0)
                .ToList();
        }

        return list;
    }
}
