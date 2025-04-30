using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using DM2Projekt.Data;
using DM2Projekt.Models;
using DM2Projekt.Models.Enums;

namespace DM2Projekt.Pages.GroupInvitations;

public class CreateModel : PageModel
{
    private readonly DM2ProjektContext _context;

    public CreateModel(DM2ProjektContext context)
    {
        _context = context;
    }

    [BindProperty]
    public GroupInvitation GroupInvitation { get; set; } = default!;

    public Group? Group { get; set; }
    public SelectList EligibleUsers { get; set; } = default!;

    public async Task<IActionResult> OnGetAsync(int? groupId)
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null || groupId == null)
            return RedirectToPage("/Login");

        // load the group and its members
        Group = await _context.Group
            .Include(g => g.UserGroups)
            .FirstOrDefaultAsync(g => g.GroupId == groupId);

        // make sure the user owns the group
        if (Group == null || Group.CreatedByUserId != userId)
            return RedirectToPage("/Groups/Index");

        var groupMemberIds = Group.UserGroups.Select(ug => ug.UserId);

        // find users who already have a pending invite
        var pendingInviteIds = await _context.GroupInvitation
            .Where(i => i.GroupId == groupId && i.IsAccepted == null)
            .Select(i => i.InvitedUserId)
            .ToListAsync();

        // show only students not already in or invited
        var eligibleUsers = await _context.User
            .Where(u => u.Role == Role.Student &&
                        !groupMemberIds.Contains(u.UserId) &&
                        !pendingInviteIds.Contains(u.UserId))
            .ToListAsync();

        EligibleUsers = new SelectList(eligibleUsers, "UserId", "Email");

        // prep form data
        GroupInvitation = new GroupInvitation
        {
            GroupId = Group.GroupId,
            SentAt = DateTime.Now,
            IsAccepted = null
        };

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null)
            return RedirectToPage("/Login");

        var group = await _context.Group.FindAsync(GroupInvitation.GroupId);
        if (group == null || group.CreatedByUserId != userId)
            return RedirectToPage("/Groups/Index");

        // don’t let them re-invite someone already pending
        bool alreadyPending = await _context.GroupInvitation
            .AnyAsync(i => i.GroupId == GroupInvitation.GroupId &&
                           i.InvitedUserId == GroupInvitation.InvitedUserId &&
                           i.IsAccepted == null);

        if (alreadyPending)
        {
            ModelState.AddModelError(string.Empty, "This user already has a pending invitation.");
            return await OnGetAsync(GroupInvitation.GroupId);
        }

        // all good — save it
        GroupInvitation.SentAt = DateTime.Now;
        GroupInvitation.IsAccepted = null;

        _context.GroupInvitation.Add(GroupInvitation);
        await _context.SaveChangesAsync();

        return RedirectToPage("/Groups/Details", new { id = GroupInvitation.GroupId });
    }
}
