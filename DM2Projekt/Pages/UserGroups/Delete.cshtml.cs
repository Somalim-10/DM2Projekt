using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using DM2Projekt.Data;
using DM2Projekt.Models;

namespace DM2Projekt.Pages.UserGroups;

public class DeleteModel : PageModel
{
    private readonly DM2ProjektContext _context;

    public DeleteModel(DM2ProjektContext context)
    {
        _context = context;
    }

    [BindProperty]
    public UserGroup UserGroup { get; set; } = default!;

    public async Task<IActionResult> OnGetAsync(int? userId, int? groupId)
    {
        if (userId == null || groupId == null)
            return NotFound();

        // find the user group
        UserGroup = await _context.UserGroup
            .Include(ug => ug.User)
            .Include(ug => ug.Group)
            .FirstOrDefaultAsync(m => m.UserId == userId && m.GroupId == groupId);

        if (UserGroup == null)
            return NotFound();

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int? userId, int? groupId)
    {
        if (userId == null || groupId == null)
            return NotFound();

        var userRole = HttpContext.Session.GetString("UserRole");

        // only Admins can actually delete
        if (userRole != "Admin")
        {
            return RedirectToPage("./Index");
        }

        var userGroup = await _context.UserGroup.FindAsync(userId, groupId);

        if (userGroup != null)
        {
            _context.UserGroup.Remove(userGroup);
            await _context.SaveChangesAsync();
        }

        return RedirectToPage("./Index");
    }
}
