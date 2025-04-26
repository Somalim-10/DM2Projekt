using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using DM2Projekt.Data;
using DM2Projekt.Models;

namespace DM2Projekt.Pages.Rooms;

public class IndexModel : PageModel
{
    private readonly DM2ProjektContext _context;

    public IndexModel(DM2ProjektContext context)
    {
        _context = context;
    }

    public IList<Room> Room { get; set; } = default!;

    public async Task<IActionResult> OnGetAsync()
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null) // if not logged in
        {
            return RedirectToPage("/Login");
        }

        Room = await _context.Room.ToListAsync();
        return Page();
    }
}
