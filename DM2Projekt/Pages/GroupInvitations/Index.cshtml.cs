using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using DM2Projekt.Data;
using DM2Projekt.Models;

namespace DM2Projekt.Pages.GroupInvitations;

public class IndexModel : PageModel
{
    private readonly DM2ProjektContext _context;

    public IndexModel(DM2ProjektContext context)
    {
        _context = context;
    }

    // all pending invites for this student
    public IList<GroupInvitation> GroupInvitation { get; set; } = [];

    // id of invite being accepted/declined
    [BindProperty]
    public int ActionInvitationId { get; set; }

    // flash message (after accept/decline)
    [TempData]
    public string? SuccessMessage { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        var role = HttpContext.Session.GetString("UserRole");

        // only students can view invites
        if (!IsStudent(userId, role))
            return RedirectToPage("/Index");

        // load their pending invites
        GroupInvitation = await GetPendingInvitesAsync(userId.Value);
        return Page();
    }

    public async Task<IActionResult> OnPostAcceptAsync()
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null)
            return RedirectToPage("/Login");

        // too many groups? stop right there
        if (await UserIsInTooManyGroups(userId.Value))
        {
            ModelState.AddModelError(string.Empty, "You cannot join more than 3 groups.");
            GroupInvitation = await GetPendingInvitesAsync(userId.Value);
            return Page();
        }

        // find that invite
        var invite = await GetInviteAsync(userId.Value);
        if (invite == null)
            return RedirectToPage();

        // cool, accept it + link user to group
        invite.IsAccepted = true;
        _context.UserGroup.Add(new UserGroup
        {
            UserId = userId.Value,
            GroupId = invite.GroupId
        });

        await _context.SaveChangesAsync();

        // success feedback
        SuccessMessage = $"You joined the group \"{invite.Group.GroupName}\"!";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeclineAsync()
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null)
            return RedirectToPage("/Login");

        var invite = await GetInviteAsync(userId.Value);
        if (invite == null)
            return RedirectToPage();

        // just mark it as declined
        invite.IsAccepted = false;
        await _context.SaveChangesAsync();

        SuccessMessage = $"You declined the invite to \"{invite.Group.GroupName}\".";
        return RedirectToPage();
    }

    // === helpers below ===

    private bool IsStudent(int? userId, string? role) =>
        userId != null && role == "Student";

    // get all invites for the user that haven't been accepted/declined
    private async Task<IList<GroupInvitation>> GetPendingInvitesAsync(int userId)
    {
        return await _context.GroupInvitation
            .Include(i => i.Group)
                .ThenInclude(g => g.CreatedByUser)
            .Where(i => i.InvitedUserId == userId && i.IsAccepted == null)
            .OrderByDescending(i => i.SentAt) // show newest invites first
            .ToListAsync();
    }

    // check if user is already in 3 groups
    private async Task<bool> UserIsInTooManyGroups(int userId)
    {
        int count = await _context.UserGroup.CountAsync(ug => ug.UserId == userId);
        return count >= 3;
    }

    // get the specific invite that matches the button click
    private async Task<GroupInvitation?> GetInviteAsync(int userId)
    {
        return await _context.GroupInvitation
            .Include(i => i.Group)
                .ThenInclude(g => g.CreatedByUser)
            .FirstOrDefaultAsync(i =>
                i.InvitationId == ActionInvitationId &&
                i.InvitedUserId == userId &&
                i.IsAccepted == null);
    }
}
