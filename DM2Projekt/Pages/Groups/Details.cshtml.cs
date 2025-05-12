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

    // the group being viewed
    public Group Group { get; set; } = default!;

    // who's in the group right now
    public List<User> Members { get; set; } = new();

    // any invites that haven't been accepted/declined yet
    public List<GroupInvitation> PendingInvites { get; set; } = new();

    // fields used by the post actions below
    [BindProperty] public int LeaveGroupId { get; set; }
    [BindProperty] public int KickUserId { get; set; }
    [BindProperty] public int KickGroupId { get; set; }
    [BindProperty] public int CancelInviteId { get; set; }

    // used for feedback after an action
    [TempData] public string? SuccessMessage { get; set; }

    public async Task<IActionResult> OnGetAsync(int? id)
    {
        if (id == null)
            return NotFound();

        // load group + members + creator
        var group = await _context.Group
            .Include(g => g.UserGroups).ThenInclude(ug => ug.User)
            .Include(g => g.CreatedByUser)
            .FirstOrDefaultAsync(g => g.GroupId == id);

        if (group == null)
            return NotFound();

        var userId = HttpContext.Session.GetInt32("UserId");
        var role = HttpContext.Session.GetString("UserRole");

        // check who this user is
        var isMember = group.UserGroups.Any(ug => ug.UserId == userId);
        var isCreator = group.CreatedByUserId == userId;
        var isAdmin = role == "Admin";

        // only allow access to people involved (or teachers/admins)
        var allowed = isMember || isCreator || isAdmin || role == "Teacher";
        if (!allowed)
            return RedirectToPage("/Groups/Index");

        Group = group;
        Members = group.UserGroups.Select(ug => ug.User).ToList();

        // if you're the group creator, you can see invites
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

    // === student leaves group ===
    public async Task<IActionResult> OnPostLeaveAsync()
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null)
            return RedirectToPage("/Login");

        var group = await _context.Group.FindAsync(LeaveGroupId);

        // group must exist and user can't leave their own group
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

    // === group creator kicks a member ===
    public async Task<IActionResult> OnPostKickAsync()
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null)
            return RedirectToPage("/Login");

        var group = await _context.Group.FindAsync(KickGroupId);

        // can only kick if you're the creator
        if (group == null || group.CreatedByUserId != userId)
            return RedirectToPage("/Groups/Index");

        // can't kick yourself!
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

    // === group creator cancels an invite ===
    public async Task<IActionResult> OnPostCancelInviteAsync()
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null)
            return RedirectToPage("/Login");

        var invite = await _context.GroupInvitation
            .Include(i => i.Group)
            .Include(i => i.InvitedUser)
            .FirstOrDefaultAsync(i => i.InvitationId == CancelInviteId);

        // invite must exist and belong to the current user's group
        if (invite == null || invite.Group.CreatedByUserId != userId)
            return RedirectToPage("/Groups/Index");

        _context.GroupInvitation.Remove(invite);
        await _context.SaveChangesAsync();

        SuccessMessage = $"You cancelled the invite to {invite.InvitedUser?.Email}.";
        return RedirectToPage(new { id = invite.GroupId });
    }
}
