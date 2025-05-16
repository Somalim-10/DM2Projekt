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
    public List<GroupInvitation> PendingInvites { get; set; } = new();

    // used for actions (leave, kick, cancel invite)
    [BindProperty] public int LeaveGroupId { get; set; }
    [BindProperty] public int KickUserId { get; set; }
    [BindProperty] public int KickGroupId { get; set; }
    [BindProperty] public int CancelInviteId { get; set; }

    [TempData] public string? SuccessMessage { get; set; }

    public async Task<IActionResult> OnGetAsync(int? id)
    {
        if (id == null)
            return NotFound();

        // pull full group info incl. creator + members
        var group = await _context.Group
            .Include(g => g.UserGroups).ThenInclude(ug => ug.User)
            .Include(g => g.CreatedByUser)
            .FirstOrDefaultAsync(g => g.GroupId == id);

        if (group == null)
            return NotFound();

        var userId = HttpContext.Session.GetInt32("UserId");
        var role = HttpContext.Session.GetString("UserRole");

        var isMember = group.UserGroups.Any(ug => ug.UserId == userId);
        var isCreator = group.CreatedByUserId == userId;
        var isAdmin = role == "Admin";
        var isTeacher = role == "Teacher";

        // you can view if you're involved in any way (or admin/teacher)
        if (!(isMember || isCreator || isAdmin || isTeacher))
            return RedirectToPage("/Groups/Index");

        Group = group;
        Members = group.UserGroups.Select(ug => ug.User).ToList();

        // only the owner gets to see pending invites
        if (isCreator)
        {
            PendingInvites = await _context.GroupInvitation
                .Include(i => i.InvitedUser)
                .Where(i => i.GroupId == group.GroupId && i.IsAccepted == null)
                .ToListAsync();
        }

        ViewData["CanEdit"] = isAdmin || isCreator;
        return Page();
    }

    // when a student leaves a group
    public async Task<IActionResult> OnPostLeaveAsync()
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null) return RedirectToPage("/Login");

        var group = await _context.Group.FindAsync(LeaveGroupId);
        if (group == null || group.CreatedByUserId == userId)
            return RedirectToPage(new { id = LeaveGroupId }); // can't leave your own group

        var membership = await _context.UserGroup
            .FirstOrDefaultAsync(ug => ug.UserId == userId && ug.GroupId == LeaveGroupId);

        if (membership != null)
        {
            _context.UserGroup.Remove(membership);
            await _context.SaveChangesAsync();
        }

        return RedirectToPage("/Groups/Index");
    }

    // when the owner kicks a member
    public async Task<IActionResult> OnPostKickAsync()
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null) return RedirectToPage("/Login");

        var group = await _context.Group.FindAsync(KickGroupId);
        if (group == null || group.CreatedByUserId != userId)
            return RedirectToPage("/Groups/Index");

        if (KickUserId == userId)
            return RedirectToPage(new { id = KickGroupId }); // can't kick yourself

        var membership = await _context.UserGroup
            .FirstOrDefaultAsync(ug => ug.UserId == KickUserId && ug.GroupId == KickGroupId);

        if (membership != null)
        {
            _context.UserGroup.Remove(membership);
            await _context.SaveChangesAsync();
        }

        return RedirectToPage(new { id = KickGroupId });
    }

    // cancel an invite if you're the owner
    public async Task<IActionResult> OnPostCancelInviteAsync()
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null) return RedirectToPage("/Login");

        var invite = await _context.GroupInvitation
            .Include(i => i.Group)
            .Include(i => i.InvitedUser)
            .FirstOrDefaultAsync(i => i.InvitationId == CancelInviteId);

        if (invite == null || invite.Group.CreatedByUserId != userId)
            return RedirectToPage("/Groups/Index");

        _context.GroupInvitation.Remove(invite);
        await _context.SaveChangesAsync();

        SuccessMessage = $"You cancelled the invite to {invite.InvitedUser?.Email}.";
        return RedirectToPage(new { id = invite.GroupId });
    }
}
