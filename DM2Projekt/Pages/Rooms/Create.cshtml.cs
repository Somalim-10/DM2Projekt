using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using DM2Projekt.Data;
using DM2Projekt.Models;

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

        _context.Room.Add(Room);
        await _context.SaveChangesAsync();

        return RedirectToPage("./Index");
    }
}
