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

        // find the user-group pair (with related info)
        UserGroup = await _context.UserGroup
            .Include(ug => ug.User)
            .Include(ug => ug.Group)
            .FirstOrDefaultAsync(ug => ug.UserId == userId && ug.GroupId == groupId);

        if (UserGroup == null)
            return NotFound();

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int? userId, int? groupId)
    {
        if (userId == null || groupId == null)
            return NotFound();

        var role = HttpContext.Session.GetString("UserRole");
        if (role != "Admin")
            return RedirectToPage("./Index");

        var userGroup = await _context.UserGroup.FindAsync(userId, groupId);

        if (userGroup != null)
        {
            _context.UserGroup.Remove(userGroup);
            await _context.SaveChangesAsync();

            TempData["Success"] = "User group was successfully deleted.";
        }

        return RedirectToPage("./Index");
    }
}
