using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using DM2Projekt.Data;
using DM2Projekt.Models;
using DM2Projekt.Models.Enums;

namespace DM2Projekt.Pages.Users;

// admin-only user overview with search + filter
public class IndexModel : PageModel
{
    private readonly DM2ProjektContext _context;

    public IndexModel(DM2ProjektContext context)
    {
        _context = context;
    }

    public IList<User> Users { get; set; } = [];

    // for search input (name/email)
    [BindProperty(SupportsGet = true)]
    public string? SearchTerm { get; set; }

    // for role dropdown filter
    [BindProperty(SupportsGet = true)]
    public Role? RoleFilter { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        // redirect if not logged in or not admin
        var userId = HttpContext.Session.GetInt32("UserId");
        var userRole = HttpContext.Session.GetString("UserRole");

        if (userId == null) return RedirectToPage("/Login");
        if (userRole != "Admin") return RedirectToPage("/Index");

        // start building the query
        var query = _context.User.AsQueryable();

        // apply search (name or email)
        if (!string.IsNullOrWhiteSpace(SearchTerm))
        {
            query = query.Where(u =>
                u.FirstName.Contains(SearchTerm) ||
                u.LastName.Contains(SearchTerm) ||
                u.Email.Contains(SearchTerm));
        }

        // apply role filter
        if (RoleFilter != null)
        {
            query = query.Where(u => u.Role == RoleFilter);
        }

        // get the final list
        Users = await query.ToListAsync();
        return Page();
    }
}
