using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using DM2Projekt.Data;
using DM2Projekt.Models;

namespace DM2Projekt.Pages.Groups;

public class DetailsModel : PageModel
{
    private readonly DM2ProjektContext _context;

    public DetailsModel(DM2ProjektContext context)
    {
        _context = context;
    }

    public Group Group { get; set; } = default!;
    public List<User> Members { get; set; } = new List<User>();

    public async Task<IActionResult> OnGetAsync(int? id)
    {
        if (id == null)
            return NotFound();

        var group = await _context.Group
            .Include(g => g.UserGroups)
                .ThenInclude(ug => ug.User)
            .Include(g => g.CreatedByUser)
            .FirstOrDefaultAsync(g => g.GroupId == id);

        if (group == null)
            return NotFound();

        var userId = HttpContext.Session.GetInt32("UserId");
        var userRole = HttpContext.Session.GetString("UserRole");

        var isMember = group.UserGroups.Any(ug => ug.UserId == userId);
        var isCreator = group.CreatedByUserId == userId;
        var isAdmin = userRole == "Admin";

        // Teachers are excluded from editing, only Admin and Owner (student) can edit
        var canEdit = isAdmin || isCreator;

        // only show if they're in the group, made the group, admin, or is a teacher
        if (!(isMember || isCreator || isAdmin || userRole == "Teacher"))
            return RedirectToPage("/Groups/Index");

        Group = group;
        Members = group.UserGroups.Select(ug => ug.User).ToList();

        // Pass canEdit to the view for button rendering
        ViewData["CanEdit"] = canEdit;

        return Page();
    }
}
