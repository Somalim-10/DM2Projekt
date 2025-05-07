using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using DM2Projekt.Models;

namespace DM2Projekt.Pages.Bookings;

public class IndexModel : PageModel
{
    private readonly Data.DM2ProjektContext _context;

    public IndexModel(Data.DM2ProjektContext context)
    {
        _context = context;
    }

    public IList<Booking> Booking { get; set; } = default!;
    public List<SelectListItem> RoomOptions { get; set; } = new();

    [BindProperty(SupportsGet = true)]
    public int? RoomId { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? Status { get; set; }

    public int? SelectedRoomId => RoomId;
    public string? SelectedStatus => Status;

    public async Task<IActionResult> OnGetAsync()
    {
        if (!IsUserLoggedIn())
            return RedirectToPage("/Login");

        await LoadRoomOptionsAsync();
        Status ??= "Upcoming"; // default filter

        Booking = await GetFilteredBookingsAsync();
        return Page();
    }

    private bool IsUserLoggedIn()
    {
        return HttpContext.Session.GetInt32("UserId") != null;
    }

    private async Task LoadRoomOptionsAsync()
    {
        RoomOptions = await _context.Room
            .OrderBy(r => r.RoomName)
            .Select(r => new SelectListItem
            {
                Value = r.RoomId.ToString(),
                Text = r.RoomName
            })
            .ToListAsync();
    }

    private async Task<List<Booking>> GetFilteredBookingsAsync()
    {
        var now = DateTime.Now;

        var query = _context.Booking
            .Include(b => b.Group)
            .Include(b => b.Room)
            .Include(b => b.CreatedByUser)
            .AsQueryable();

        if (RoomId.HasValue)
            query = query.Where(b => b.RoomId == RoomId.Value);

        if (!string.IsNullOrEmpty(Status))
        {
            query = Status switch
            {
                "Upcoming" => query.Where(b => b.StartTime > now),
                "Ongoing" => query.Where(b => b.StartTime <= now && b.EndTime > now),
                "Past" => query.Where(b => b.EndTime <= now),
                _ => query
            };
        }

        return await query
            .OrderBy(b => b.StartTime)
            .AsNoTracking()
            .ToListAsync();
    }
}
