using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using DM2Projekt.Data;
using DM2Projekt.Models;
using DM2Projekt.Models.Enums;

namespace DM2Projekt.Pages.Users;

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
        var userId = HttpContext.Session.GetInt32("UserId");
        var userRole = HttpContext.Session.GetString("UserRole");

        if (userId == null) // not logged in
            return RedirectToPage("/Login");

        if (userRole != "Admin") // only Admin can create users
            return RedirectToPage("/Index");

        // load role options
        RoleOptions = Enum.GetValues(typeof(Role))
                          .Cast<Role>()
                          .Select(r => new SelectListItem
                          {
                              Value = ((int)r).ToString(),
                              Text = r.ToString()
                          });

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        var userRole = HttpContext.Session.GetString("UserRole");

        if (userId == null) // not logged in
            return RedirectToPage("/Login");

        if (userRole != "Admin") // only Admin can create users
            return RedirectToPage("/Index");

        if (!ModelState.IsValid)
        {
            // reload role options if invalid
            RoleOptions = Enum.GetValues(typeof(Role))
                              .Cast<Role>()
                              .Select(r => new SelectListItem
                              {
                                  Value = ((int)r).ToString(),
                                  Text = r.ToString()
                              });

            return Page();
        }

        // save new user
        _context.User.Add(User);
        await _context.SaveChangesAsync();

        return RedirectToPage("./Index");
    }
}
