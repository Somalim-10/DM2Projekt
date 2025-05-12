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

    public SelectList Users { get; set; } = default!;
    public SelectList Groups { get; set; } = default!;

    public IActionResult OnGet()
    {
        var role = HttpContext.Session.GetString("UserRole");

        // only admins can get in here
        if (role != "Admin")
            return RedirectToPage("./Index");

        LoadDropdowns();
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var role = HttpContext.Session.GetString("UserRole");

        if (role != "Admin")
            return RedirectToPage("./Index");

        if (!ModelState.IsValid)
        {
            LoadDropdowns();
            return Page();
        }

        _context.UserGroup.Add(UserGroup);
        await _context.SaveChangesAsync();

        TempData["Success"] = "User group was successfully created.";
        return RedirectToPage("./Index");
    }

    private void LoadDropdowns()
    {
        // sorted by name for easier admin selection
        Users = new SelectList(_context.User.OrderBy(u => u.Email), "UserId", "Email");
        Groups = new SelectList(_context.Group.OrderBy(g => g.GroupName), "GroupId", "GroupName");

        ViewData["UserId"] = Users;
        ViewData["GroupId"] = Groups;
    }
}
