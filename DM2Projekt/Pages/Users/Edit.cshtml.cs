using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using DM2Projekt.Data;
using DM2Projekt.Models;

namespace DM2Projekt.Pages.Users;

// admin-only page for editing a user's info
public class EditModel : PageModel
{
    private readonly DM2ProjektContext _context;

    public EditModel(DM2ProjektContext context)
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

        // only logged-in admins can edit users
        if (userId == null) return RedirectToPage("/Login");
        if (userRole != "Admin") return RedirectToPage("/Index");

        var user = await _context.User.FirstOrDefaultAsync(m => m.UserId == id);
        if (user == null) return NotFound();

        User = user;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        // we're not editing the password, so skip its validation
        ModelState.Remove("User.Password");

        if (!ModelState.IsValid) return Page();

        var userId = HttpContext.Session.GetInt32("UserId");
        var userRole = HttpContext.Session.GetString("UserRole");

        if (userId == null) return RedirectToPage("/Login");
        if (userRole != "Admin") return RedirectToPage("/Index");

        // load existing user from DB
        var existingUser = await _context.User.FirstOrDefaultAsync(u => u.UserId == User.UserId);
        if (existingUser == null) return NotFound();

        // update only allowed fields
        existingUser.FirstName = User.FirstName;
        existingUser.LastName = User.LastName;
        existingUser.Email = User.Email;
        existingUser.Role = User.Role;

        await _context.SaveChangesAsync();
        return RedirectToPage("./Index");
    }
}
