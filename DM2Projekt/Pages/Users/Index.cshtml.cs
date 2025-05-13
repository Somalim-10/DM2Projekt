using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using DM2Projekt.Data;
using DM2Projekt.Models;
using DM2Projekt.Models.Enums;

namespace DM2Projekt.Pages.Users;

// this page shows a list of users (admin-only)
// has basic search + filtering by role
public class IndexModel : PageModel
{
    private readonly DM2ProjektContext _context;

    public IndexModel(DM2ProjektContext context)
    {
        _context = context;
    }

    public IList<User> Users { get; set; } = [];

    // search input (by name/email)
    [BindProperty(SupportsGet = true)]
    public string? SearchTerm { get; set; }

    // dropdown role filter
    [BindProperty(SupportsGet = true)]
    public Role? RoleFilter { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        // redirect if not logged in or not admin
        var userId = HttpContext.Session.GetInt32("UserId");
        var userRole = HttpContext.Session.GetString("UserRole");

        if (userId == null) return RedirectToPage("/Login");
        if (userRole != "Admin") return RedirectToPage("/Index");

        // start with all users
        var query = _context.User.AsQueryable();

        // filter by search (first, last, or email)
        if (!string.IsNullOrWhiteSpace(SearchTerm))
        {
            query = query.Where(u =>
                u.FirstName.Contains(SearchTerm) ||
                u.LastName.Contains(SearchTerm) ||
                u.Email.Contains(SearchTerm));
        }

        // filter by role
        if (RoleFilter != null)
        {
            query = query.Where(u => u.Role == RoleFilter);
        }

        // final list
        Users = await query.ToListAsync();
        return Page();
    }
}
