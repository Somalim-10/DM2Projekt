using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using DM2Projekt.Data;
using DM2Projekt.Models;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using Microsoft.AspNetCore.Mvc.Rendering;
using DM2Projekt.Models.Enums;

namespace DM2Projekt.Pages.Rooms;

public class IndexModel : PageModel
{
    private readonly DM2ProjektContext _context;

    public IndexModel(DM2ProjektContext context)
    {
        _context = context;
    }

    public List<SelectListItem> RoomOptions { get; set; } = new();// holds all room choices


    public List<SelectListItem> BuildingOptions { get; set; } = new();
    public List<SelectListItem> FloorOptions { get; set; } = new();
    public List<SelectListItem> RoomTypeOptions { get; set; } = new();

    [BindProperty(SupportsGet = true)]
    public Building? Building { get; set; }

    [BindProperty(SupportsGet = true)]
    public Floor? Floor { get; set; }

    [BindProperty(SupportsGet = true)]
    public RoomType? RoomType { get; set; }  

    [BindProperty(SupportsGet = true)]
    public int? RoomId { get; set; }
    public int? SelectedRoomId => RoomId;



    public IList<Room> Room { get; set; } = default!;

    public IQueryable<Room> FilterRooms(IQueryable<Room> query, int? roomId)
    {
        if (roomId.HasValue)
        {
            query = query.Where(r => r.RoomId == roomId.Value);
        }
        return query;
    }

    public async Task<IActionResult> OnGetAsync()
    {
        // Hent dropdown for rum
        RoomOptions = await _context.Room
            .OrderBy(r => r.RoomName)
            .Select(r => new SelectListItem
            {
                Value = r.RoomId.ToString(),
                Text = r.RoomName
            })
            .ToListAsync();

        // Hent distinct Buildings (enum)
        var buildingValues = await _context.Room
            .Select(r => r.Building)
            .Distinct()
            .ToListAsync();

        BuildingOptions = buildingValues
            .OrderBy(b => b)
            .Select(b => new SelectListItem
            {
                Value = ((int)b).ToString(),
                Text = b.ToString()
            })
            .ToList();

        var roomTypeValues = await _context.Room
            .Select(r => r.RoomType)
            .Distinct()
            .ToListAsync();


        RoomTypeOptions = roomTypeValues
           .OrderBy(rt=> rt)
           .Select(rt => new SelectListItem
           {
               Value = ((int)rt).ToString(),
               Text = rt.ToString()
           })
           .ToList();

        // Hent distinct Floors (enum)
        var floorValues = await _context.Room
            .Select(r => r.Floor)
            .Distinct()
            .ToListAsync();

        FloorOptions = floorValues
            .OrderBy(f => f)
            .Select(f => new SelectListItem
            {
                Value = ((int)f).ToString(),
                Text = f.ToString()
            })
            .ToList();

        // Byg query og anvend filtre
        var query = _context.Room.AsQueryable();

        if (RoomId.HasValue)
        {
            query = query.Where(r => r.RoomId == RoomId.Value);
        }

        if (Building.HasValue)
        {
            query = query.Where(r => r.Building == Building.Value);
        }

        if (RoomType.HasValue)
        {
            query = query.Where(r => r.RoomType == RoomType.Value);
        }

        if (Floor.HasValue)
        {
            query = query.Where(r => r.Floor == Floor.Value);
        }

        // Tjek login
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null)
        {
            return RedirectToPage("/Login");
        }

        // Udfør query og send data til view
        Room = await query.ToListAsync();

        return Page();
    }

}
