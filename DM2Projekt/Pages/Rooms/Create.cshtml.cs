using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using DM2Projekt.Data;
using DM2Projekt.Models;
using Microsoft.EntityFrameworkCore;

namespace DM2Projekt.Pages.Rooms;

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

    public string? NewProfileImageUrl { get; set; }

    public string ProfilePictureMessage { get; set; } = "";
    public bool ProfilePictureSuccess { get; set; } = false;


    public IActionResult OnGet()
    {
        var userRole = HttpContext.Session.GetString("UserRole");

        // only Admins can access Create page
        if (userRole != "Admin")
            return RedirectToPage("/Rooms/Index");

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var userRole = HttpContext.Session.GetString("UserRole");

        // only Admins can create
        if (userRole != "Admin")
            return RedirectToPage("/Rooms/Index");

        if (!ModelState.IsValid)
            return Page();

        bool roomExists = await _context.Room
        .AnyAsync(r => r.RoomName.ToLower() == Room.RoomName.ToLower());

        if (roomExists)
        {
            ModelState.AddModelError("Room.RoomName", "Et rum med dette navn findes allerede.");
            return Page();
        }

        if (!string.IsNullOrWhiteSpace(NewProfileImageUrl))
        {
            if (!await UrlExistsAsync(NewProfileImageUrl))
            {
                ProfilePictureMessage = "The image URL seems to be broken or inaccessible.";
                ProfilePictureSuccess = false;
                return Page(); // afbryd og vis fejl
            }

            // ✅ URL'en er valid – sæt den på Room
            Room.ImageUrl = NewProfileImageUrl;
        }

        _context.Room.Add(Room);
        await _context.SaveChangesAsync();

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
}
