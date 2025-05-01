using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using DM2Projekt.Data;
using DM2Projekt.Models;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace DM2Projekt.Pages.Rooms;

public class IndexModel : PageModel
{
    private readonly DM2ProjektContext _context;

    public IndexModel(DM2ProjektContext context)
    {
        _context = context;
    }

    public List<SelectListItem> RoomOptions { get; set; } = new(); // holds all room choices

    [BindProperty(SupportsGet = true)]
    public int? RoomId { get; set; }
    public int? SelectedRoomId => RoomId;



    public IList<Room> Room { get; set; } = default!;

    public async Task<IActionResult> OnGetAsync()
    {

        RoomOptions = await _context.Room
          .OrderBy(r => r.RoomName)
          .Select(r => new SelectListItem
          {
              Value = r.RoomId.ToString(),
              Text = r.RoomName
          })
          .ToListAsync();
   
        var query = _context.Room.AsQueryable();
         
       
        if (RoomId.HasValue)
        {
            query = query.Where(b => b.RoomId == RoomId.Value);
        }


        if (RoomId.HasValue)
        {
            query = query.Where(b => b.RoomId == RoomId.Value);
        }


        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null) // if not logged in
        {
            return RedirectToPage("/Login");
        }

        Room = await query.ToListAsync();
        return Page();
    }
}
