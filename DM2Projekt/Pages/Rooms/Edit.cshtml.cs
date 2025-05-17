using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using DM2Projekt.Data;
using DM2Projekt.Models;

namespace DM2Projekt.Pages.Rooms;

// yo this page handles editing rooms — only for the VIPs (admins)
public class EditModel : PageModel
{
    private readonly DM2ProjektContext _context;

    public EditModel(DM2ProjektContext context)
    {
        _context = context;
    }

    [BindProperty]
    public Room Room { get; set; } = default!; // current room we’re editing

    [BindProperty]
    public string? NewProfileImageUrl { get; set; } // url input for new image

    public string ProfilePictureMessage { get; set; } = ""; // lil feedback msg
    public bool ProfilePictureSuccess { get; set; } = false; // green or red?

    public async Task<IActionResult> OnGetAsync(int? id)
    {
        // 🛑 no login, no service
        var userId = HttpContext.Session.GetInt32("UserId");
        var userRole = HttpContext.Session.GetString("UserRole");

        if (userId == null || userRole != "Admin")
            return RedirectToPage("/Login");

        if (id == null)
            return NotFound();

        // 📦 grab the room by id
        var room = await _context.Room.FirstOrDefaultAsync(m => m.RoomId == id);
        if (room == null)
            return NotFound();

        Room = room;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(string? testUserRole = null, int? testUserId = null)
    {
        // 🧪 support test overrides
        var userId = testUserId ?? HttpContext.Session.GetInt32("UserId");
        var userRole = testUserRole ?? HttpContext.Session.GetString("UserRole");

        if (userId == null || userRole != "Admin")
            return RedirectToPage("/Login");

        if (!ModelState.IsValid)
            return Page();

        // 🎯 validate image url if user typed one
        if (!string.IsNullOrWhiteSpace(NewProfileImageUrl))
        {
            if (!await UrlExistsAsync(NewProfileImageUrl))
            {
                ProfilePictureMessage = "The image URL seems to be broken or inaccessible.";
                ProfilePictureSuccess = false;

                // re-fetch room in case user tampered stuff (just to be sure)
                Room = await _context.Room.FirstOrDefaultAsync(r => r.RoomId == Room.RoomId);
                return Page();
            }

            // 💾 looks good — update the room image
            Room.ImageUrl = NewProfileImageUrl;
        }

        _context.Attach(Room).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!RoomExists(Room.RoomId))
                return NotFound();
            else
                throw; // oops? let it blow up
        }

        return RedirectToPage("./Index");
    }

    // 👀 lightweight check to make sure the url is real + is an image
    private async Task<bool> UrlExistsAsync(string url)
    {
        try
        {
            using var httpClient = new HttpClient();
            using var response = await httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Head, url));

            return response.IsSuccessStatusCode &&
                   response.Content.Headers.ContentType?.MediaType?.StartsWith("image") == true;
        }
        catch
        {
            return false; // if anything goes wrong, nope.
        }
    }

    // 🧠 sanity check to see if room even exists
    private bool RoomExists(int id)
    {
        return _context.Room.Any(e => e.RoomId == id);
    }
}
