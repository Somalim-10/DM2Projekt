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
    public List<User> Members { get; set; } = new();

    [BindProperty]
    public int LeaveGroupId { get; set; }

    [BindProperty]
    public int KickUserId { get; set; }

    [BindProperty]
    public int KickGroupId { get; set; }

    [TempData]
    public string? SuccessMessage { get; set; }

    public async Task<IActionResult> OnGetAsync(int? id)
    {
        if (id == null)
            return NotFound();

        // get group with members and creator
        var group = await _context.Group
            .Include(g => g.UserGroups).ThenInclude(ug => ug.User)
            .Include(g => g.CreatedByUser)
            .FirstOrDefaultAsync(g => g.GroupId == id);

        if (group == null)
            return NotFound();

        var userId = HttpContext.Session.GetInt32("UserId");
        var userRole = HttpContext.Session.GetString("UserRole");

        var isMember = group.UserGroups.Any(ug => ug.UserId == userId);
        var isCreator = group.CreatedByUserId == userId;
        var isAdmin = userRole == "Admin";

        // allow: members, creator, admins, teachers
        var allowed = isMember || isCreator || isAdmin || userRole == "Teacher";
        if (!allowed)
            return RedirectToPage("/Groups/Index");

        Group = group;
        Members = group.UserGroups.Select(ug => ug.User).ToList();

        ViewData["CanEdit"] = isAdmin || isCreator;

        return Page();
    }

    public async Task<IActionResult> OnPostLeaveAsync()
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null)
            return RedirectToPage("/Login");

        var group = await _context.Group.FindAsync(LeaveGroupId);
        if (group == null || group.CreatedByUserId == userId)
            return RedirectToPage(new { id = LeaveGroupId });

        var membership = await _context.UserGroup
            .FirstOrDefaultAsync(ug => ug.UserId == userId && ug.GroupId == LeaveGroupId);

        if (membership != null)
        {
            _context.UserGroup.Remove(membership);
            await _context.SaveChangesAsync();
        }

        return RedirectToPage("/Groups/Index");
    }

    public async Task<IActionResult> OnPostKickAsync()
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null)
            return RedirectToPage("/Login");

        var group = await _context.Group.FindAsync(KickGroupId);
        if (group == null || group.CreatedByUserId != userId)
            return RedirectToPage("/Groups/Index");

        // 🛑 can't kick yourself
        if (KickUserId == userId)
            return RedirectToPage(new { id = KickGroupId });

        var membership = await _context.UserGroup
            .FirstOrDefaultAsync(ug => ug.UserId == KickUserId && ug.GroupId == KickGroupId);

        if (membership != null)
        {
            _context.UserGroup.Remove(membership);
            await _context.SaveChangesAsync();
        }

        return RedirectToPage(new { id = KickGroupId });
    }
}
