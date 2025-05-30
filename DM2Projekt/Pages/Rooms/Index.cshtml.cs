using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using DM2Projekt.Data;
using DM2Projekt.Models;

namespace DM2Projekt.Pages.Rooms;

public class IndexModel : PageModel
{
    private readonly DM2ProjektContext _context;

    public IndexModel(DM2ProjektContext context)
        => _context = context; // injected context

    // filters from query string
    [BindProperty(SupportsGet = true)] public string? SearchTerm { get; set; }
    [BindProperty(SupportsGet = true)] public Building? Building { get; set; }
    [BindProperty(SupportsGet = true)] public Floor? Floor { get; set; }
    [BindProperty(SupportsGet = true)] public RoomType? RoomType { get; set; }

    // for dropdowns
    public List<SelectListItem> BuildingOptions { get; set; } = new();
    public List<SelectListItem> FloorOptions { get; set; } = new();
    public List<SelectListItem> RoomTypeOptions { get; set; } = new();

    // actual rooms shown in UI
    public List<Room> Rooms { get; set; } = new();

    public async Task<IActionResult> OnGetAsync()
    {
        // make sure you're logged in
        if (HttpContext.Session.GetInt32("UserId") == null)
            return RedirectToPage("/Login");

        await LoadDropdownOptionsAsync();
        Rooms = await GetFilteredRoomsAsync();

        return Page();
    }

    // loads dropdown options from all rooms in DB
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

    // apply filters and return list
    private async Task<List<Room>> GetFilteredRoomsAsync()
    {
        var query = _context.Room.AsQueryable();

        if (Building.HasValue)
            query = query.Where(r => r.Building == Building.Value);

        if (Floor.HasValue)
            query = query.Where(r => r.Floor == Floor.Value);

        if (RoomType.HasValue)
            query = query.Where(r => r.RoomType == RoomType.Value);

        var list = await query.ToListAsync();

        // basic string contains, case-insensitive
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
