using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using DM2Projekt.Data;
using DM2Projekt.Models;

namespace DM2Projekt.Pages.Rooms;

public class EditModel : PageModel
{
    private readonly DM2ProjektContext _context;

    public EditModel(DM2ProjektContext context)
    {
        _context = context;
    }

    [BindProperty]
    public Room Room { get; set; } = default!;

    [BindProperty]
    public string? NewProfileImageUrl { get; set; }

    public string ProfilePictureMessage { get; set; } = "";
    public bool ProfilePictureSuccess { get; set; } = false;


    public async Task<IActionResult> OnGetAsync(int? id)
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        var userRole = HttpContext.Session.GetString("UserRole");

        // must be logged in and be Admin
        if (userId == null || userRole != "Admin")
            return RedirectToPage("/Login");

        if (id == null)
            return NotFound();

        var room = await _context.Room.FirstOrDefaultAsync(m => m.RoomId == id);
        if (room == null)
            return NotFound();

        Room = room;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        var userRole = HttpContext.Session.GetString("UserRole");

        // must be logged in and be Admin
        if (userId == null || userRole != "Admin")
            return RedirectToPage("/Login");

        if (!ModelState.IsValid)
            return Page();

        if (!string.IsNullOrWhiteSpace(NewProfileImageUrl))
        {
            if (!await UrlExistsAsync(NewProfileImageUrl))
            {
                ProfilePictureMessage = "The image URL seems to be broken or inaccessible.";
                ProfilePictureSuccess = false;
                Room = await _context.Room.FirstOrDefaultAsync(r => r.RoomId == Room.RoomId); // henter rummet igen
                return Page();
            }

            // 🟢 heeerrr – opdater ImageUrl, da URL'en er valid
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
                throw;
        }

        return RedirectToPage("./Index");
    }
    private async Task<bool> UrlExistsAsync(string url)
    {
        try
        {
            using var httpClient = new HttpClient();

            // Send a lightweight "HEAD" request – we just want the headers, not the full image
            using var response = await httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Head, url));

            // Return true if the URL exists AND it's actually an image
            return response.IsSuccessStatusCode &&
                   response.Content.Headers.ContentType?.MediaType?.StartsWith("image") == true;
        }
        catch
        {
            // If the request fails (404, timeout, bad URL, etc), just say nope
            return false;
        }
    }

    // helper to check if room still exists
    private bool RoomExists(int id)
    {
        return _context.Room.Any(e => e.RoomId == id);
    }
}
