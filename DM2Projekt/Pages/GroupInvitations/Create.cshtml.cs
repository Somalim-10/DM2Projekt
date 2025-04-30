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

        // Load the group
        Group = await _context.Group
            .Include(g => g.UserGroups)
            .FirstOrDefaultAsync(g => g.GroupId == groupId);

        if (Group == null || Group.CreatedByUserId != userId)
            return RedirectToPage("/Groups/Index"); // not allowed

        // Get users who are students, not already in the group, and not already invited
        var invitedIds = _context.GroupInvitation
            .Where(i => i.GroupId == groupId)
            .Select(i => i.InvitedUserId);

        var groupMemberIds = Group.UserGroups.Select(ug => ug.UserId);

        var eligibleUsers = await _context.User
            .Where(u => u.Role == Role.Student &&
                        !groupMemberIds.Contains(u.UserId) &&
                        !invitedIds.Contains(u.UserId))
            .ToListAsync();

        EligibleUsers = new SelectList(eligibleUsers, "UserId", "Email");

        // Pre-fill and lock group ID
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

        // Prevent duplicate invitations
        bool alreadyInvited = await _context.GroupInvitation
            .AnyAsync(i => i.GroupId == GroupInvitation.GroupId && i.InvitedUserId == GroupInvitation.InvitedUserId);

        if (alreadyInvited)
        {
            ModelState.AddModelError(string.Empty, "This user has already been invited.");
            return await OnGetAsync(GroupInvitation.GroupId); // reload view
        }

        GroupInvitation.SentAt = DateTime.Now;
        GroupInvitation.IsAccepted = null;

        _context.GroupInvitation.Add(GroupInvitation);
        await _context.SaveChangesAsync();

        return RedirectToPage("/Groups/Details", new { id = GroupInvitation.GroupId });
    }
}
