using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using DM2Projekt.Data;
using DM2Projekt.Models;
using DM2Projekt.Models.Enums;

namespace DM2Projekt.Pages.Users;

// admin-only: create a new user
public class CreateModel : PageModel
{
    private readonly DM2ProjektContext _context;

    public CreateModel(DM2ProjektContext context)
    {
        _context = context;
    }

    [BindProperty]
    public User User { get; set; } = default!;

    public IEnumerable<SelectListItem> RoleOptions { get; set; } = [];

    public IActionResult OnGet()
    {
        if (!IsAdmin()) return RedirectToPage("/Login");

        LoadRoleOptions();
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!IsAdmin()) return RedirectToPage("/Login");

        if (!ModelState.IsValid)
        {
            LoadRoleOptions(); // reload dropdown values
            return Page();
        }

        _context.User.Add(User); // save new user
        await _context.SaveChangesAsync();

        return RedirectToPage("./Index");
    }

    // helper: only allow logged-in admins
    private bool IsAdmin()
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        var userRole = HttpContext.Session.GetString("UserRole");
        return userId != null && userRole == "Admin";
    }

    // helper: populate the dropdown
    private void LoadRoleOptions()
    {
        RoleOptions = Enum.GetValues(typeof(Role))
                          .Cast<Role>()
                          .Select(r => new SelectListItem
                          {
                              Value = ((int)r).ToString(),
                              Text = r.ToString()
                          });
    }
}
