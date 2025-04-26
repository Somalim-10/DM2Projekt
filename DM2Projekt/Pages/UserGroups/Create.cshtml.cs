using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using DM2Projekt.Data;
using DM2Projekt.Models;

namespace DM2Projekt.Pages.UserGroups;

public class CreateModel : PageModel
{
    private readonly DM2ProjektContext _context;

    public CreateModel(DM2ProjektContext context)
    {
        _context = context;
    }

    [BindProperty]
    public UserGroup UserGroup { get; set; } = default!;

    public IActionResult OnGet()
    {
        var userRole = HttpContext.Session.GetString("UserRole");

        // only Admins can access create
        if (userRole != "Admin")
        {
            return RedirectToPage("./Index");
        }

        ViewData["GroupId"] = new SelectList(_context.Group, "GroupId", "GroupName");
        ViewData["UserId"] = new SelectList(_context.User, "UserId", "Email");

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var userRole = HttpContext.Session.GetString("UserRole");

        // only Admins can submit create
        if (userRole != "Admin")
        {
            return RedirectToPage("./Index");
        }

        if (!ModelState.IsValid)
        {
            return Page();
        }

        // add and save new UserGroup
        _context.UserGroup.Add(UserGroup);
        await _context.SaveChangesAsync();

        return RedirectToPage("./Index");
    }
}
