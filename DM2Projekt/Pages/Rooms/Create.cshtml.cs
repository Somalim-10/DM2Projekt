using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using DM2Projekt.Data;
using DM2Projekt.Models;
using Microsoft.EntityFrameworkCore;

namespace DM2Projekt.Pages.Rooms;

// this is the “Add New Room” page. Admins only.
public class CreateModel : PageModel
{
    private readonly DM2ProjektContext _context;

    public CreateModel(DM2ProjektContext context)
    {
        _context = context;
    }

    [BindProperty]
    public Room Room { get; set; } = default!;

    [BindProperty]
    public string? NewProfileImageUrl { get; set; } // optional image input

    public string ProfilePictureMessage { get; set; } = "";
    public bool ProfilePictureSuccess { get; set; } = false;

    public IActionResult OnGet()
    {
        var userRole = HttpContext.Session.GetString("UserRole");

        // no sneaky users allowed
        if (userRole != "Admin")
            return RedirectToPage("/Rooms/Index");

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(string? testUserRole = null, int? testUserId = null)
    {
        var userId = testUserId ?? HttpContext?.Session?.GetInt32("UserId");
        var userRole = testUserRole ?? HttpContext?.Session?.GetString("UserRole");

        if (userRole != "Admin")
            return RedirectToPage("/Rooms/Index");

        if (!ModelState.IsValid)
            return Page();

        // check if name already exists (case-insensitive)
        var existingRoomNames = await _context.Room
            .Select(r => r.RoomName)
            .ToListAsync();

        bool roomExists = existingRoomNames
            .Any(name => string.Equals(name, Room.RoomName, StringComparison.OrdinalIgnoreCase));

        if (roomExists)
        {
            ModelState.AddModelError("Room.RoomName", "Et rum med dette navn findes allerede.");
            return Page();
        }

        // validate new image URL (if given)
        if (!string.IsNullOrWhiteSpace(NewProfileImageUrl))
        {
            if (!await UrlExistsAsync(NewProfileImageUrl))
            {
                ProfilePictureMessage = "The image URL seems to be broken or inaccessible.";
                ProfilePictureSuccess = false;
                return Page();
            }

            Room.ImageUrl = NewProfileImageUrl; // good to go
        }

        _context.Room.Add(Room);
        await _context.SaveChangesAsync();

        return RedirectToPage("./Index");
    }

    // double-checks image URL actually points to an image
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
            return false; // anything goes wrong = assume it's bad
        }
    }
}
