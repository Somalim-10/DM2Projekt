using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using DM2Projekt.Data;
using DM2Projekt.Models;

namespace DM2Projekt.Pages.Users;

public class DetailsModel : PageModel
{
    private readonly DM2ProjektContext _context;

    public DetailsModel(DM2ProjektContext context)
    {
        _context = context;
    }

    public User User { get; set; } = default!;

    public async Task<IActionResult> OnGetAsync(int? id)
    {
        if (id == null) return NotFound();

        var userId = HttpContext.Session.GetInt32("UserId");
        var userRole = HttpContext.Session.GetString("UserRole");

        if (userId == null) // not logged in
            return RedirectToPage("/Login");

        if (userRole != "Admin") // only Admin can see user details
            return RedirectToPage("/Index");

        var user = await _context.User.FirstOrDefaultAsync(m => m.UserId == id);
        if (user != null)
        {
            User = user;
            return Page();
        }

        return NotFound();
    }
}
