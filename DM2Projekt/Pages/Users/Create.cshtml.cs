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
        if (!ModelState.IsValid)
        {
            return Page();
        }

        _context.User.Add(User);
        await _context.SaveChangesAsync();

        return RedirectToPage("./Index");
    }
}
