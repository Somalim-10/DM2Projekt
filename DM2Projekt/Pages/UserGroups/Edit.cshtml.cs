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

    public SelectList GroupOptions { get; set; } = default!;
    public User CurrentUser { get; set; } = default!;
    public Group CurrentGroup { get; set; } = default!;

    public async Task<IActionResult> OnGetAsync(int? userId, int? groupId)
    {
        if (userId == null || groupId == null)
            return NotFound();

        var role = HttpContext.Session.GetString("UserRole");
        if (role != "Admin")
            return RedirectToPage("/UserGroups/Index");

        // fetch the user-group link
        UserGroup = await _context.UserGroup
            .Include(ug => ug.User)
            .Include(ug => ug.Group)
            .FirstOrDefaultAsync(m => m.UserId == userId && m.GroupId == groupId);

        if (UserGroup == null)
            return NotFound();

        // fetch user + group separately for display
        CurrentUser = UserGroup.User;
        CurrentGroup = UserGroup.Group;

        // list all groups except the current one
        GroupOptions = new SelectList(_context.Group
            .OrderBy(g => g.GroupName), "GroupId", "GroupName", UserGroup.GroupId);

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var role = HttpContext.Session.GetString("UserRole");
        if (role != "Admin")
            return RedirectToPage("/UserGroups/Index");

        if (!ModelState.IsValid)
        {
            // reload dropdown in case of error
            GroupOptions = new SelectList(_context.Group.OrderBy(g => g.GroupName), "GroupId", "GroupName", UserGroup.GroupId);
            return Page();
        }

        // delete old entry
        var old = await _context.UserGroup.FindAsync(UserGroup.UserId, UserGroup.GroupId);
        if (old != null)
            _context.UserGroup.Remove(old);

        // add new link
        _context.UserGroup.Add(UserGroup);
        await _context.SaveChangesAsync();

        TempData["Success"] = "User was successfully moved to a new group.";
        return RedirectToPage("./Index");
    }
}
