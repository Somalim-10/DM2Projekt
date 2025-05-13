using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using DM2Projekt.Data;
using DM2Projekt.Models;

namespace DM2Projekt.Pages.Users;

// admin-only: confirm deletion of a user
public class DeleteModel : PageModel
{
    private readonly DM2ProjektContext _context;

    public DeleteModel(DM2ProjektContext context)
    {
        _context = context;
    }

    [BindProperty]
    public User User { get; set; } = default!;

    public async Task<IActionResult> OnGetAsync(int? id)
    {
        if (id == null) return NotFound();

        var userId = HttpContext.Session.GetInt32("UserId");
        var userRole = HttpContext.Session.GetString("UserRole");

        if (userId == null || userRole != "Admin")
            return RedirectToPage("/Login");

        var user = await _context.User.FirstOrDefaultAsync(u => u.UserId == id);
        if (user == null) return NotFound();

        User = user;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int? id)
    {
        if (id == null) return NotFound();

        var userId = HttpContext.Session.GetInt32("UserId");
        var userRole = HttpContext.Session.GetString("UserRole");

        if (userId == null || userRole != "Admin")
            return RedirectToPage("/Login");

        var user = await _context.User.FindAsync(id);
        if (user != null)
        {
            _context.User.Remove(user);
            await _context.SaveChangesAsync();
        }

        return RedirectToPage("./Index");
    }
}
