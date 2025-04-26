using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using DM2Projekt.Data;
using DM2Projekt.Models;

namespace DM2Projekt.Pages.UserGroups;

public class EditModel : PageModel
{
    private readonly DM2ProjektContext _context;

    public EditModel(DM2ProjektContext context)
    {
        _context = context;
    }

    [BindProperty]
    public UserGroup UserGroup { get; set; } = default!;

    public async Task<IActionResult> OnGetAsync(int? userId, int? groupId)
    {
        if (userId == null || groupId == null)
            return NotFound();

        var userRole = HttpContext.Session.GetString("UserRole");
        if (userRole != "Admin") // only Admins allowed
            return RedirectToPage("/UserGroups/Index");

        // load UserGroup
        UserGroup = await _context.UserGroup
            .Include(ug => ug.User)
            .Include(ug => ug.Group)
            .FirstOrDefaultAsync(m => m.UserId == userId && m.GroupId == groupId);

        if (UserGroup == null)
            return NotFound();

        // setup dropdowns
        ViewData["UserId"] = new SelectList(_context.User, "UserId", "Email", UserGroup.UserId);
        ViewData["GroupId"] = new SelectList(_context.Group, "GroupId", "GroupName", UserGroup.GroupId);

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var userRole = HttpContext.Session.GetString("UserRole");
        if (userRole != "Admin")
            return RedirectToPage("/UserGroups/Index");

        if (!ModelState.IsValid)
            return Page();

        try
        {
            _context.Update(UserGroup);
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            bool exists = _context.UserGroup.Any(e =>
                e.UserId == UserGroup.UserId && e.GroupId == UserGroup.GroupId);

            if (!exists)
                return NotFound();

            throw;
        }

        return RedirectToPage("./Index");
    }
}
