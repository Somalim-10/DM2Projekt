using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using DM2Projekt.Data;
using DM2Projekt.Models;

namespace DM2Projekt.Pages.Groups;

public class CreateModel : PageModel
{
    private readonly DM2ProjektContext _context;

    public CreateModel(DM2ProjektContext context)
    {
        _context = context;
    }

    [BindProperty]
    public Group Group { get; set; } = default!;

    public IActionResult OnGet()
    {
        var userRole = HttpContext.Session.GetString("UserRole");

        // only Admins can open Create page
        if (userRole != "Admin")
            return RedirectToPage("/Groups/Index");

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var userRole = HttpContext.Session.GetString("UserRole");

        // block non-Admins from posting
        if (userRole != "Admin")
            return RedirectToPage("/Groups/Index");

        if (!ModelState.IsValid)
            return Page();

        // save new group
        _context.Group.Add(Group);
        await _context.SaveChangesAsync();

        return RedirectToPage("./Index");
    }
}
