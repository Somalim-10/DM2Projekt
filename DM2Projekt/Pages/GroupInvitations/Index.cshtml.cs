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

    // List of pending invites for this student
    public IList<GroupInvitation> GroupInvitation { get; set; } = [];

    // Used for Accept/Decline actions
    [BindProperty]
    public int ActionInvitationId { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        var role = HttpContext.Session.GetString("UserRole");

        if (!IsStudent(userId, role))
            return RedirectToPage("/Index");

        GroupInvitation = await GetPendingInvitesAsync(userId.Value);
        return Page();
    }

    public async Task<IActionResult> OnPostAcceptAsync()
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null)
            return RedirectToPage("/Login");

        if (await UserIsInTooManyGroups(userId.Value))
        {
            ModelState.AddModelError(string.Empty, "You cannot join more than 3 groups.");
            GroupInvitation = await GetPendingInvitesAsync(userId.Value);
            return Page();
        }

        var invite = await GetInviteAsync(userId.Value);
        if (invite == null)
            return RedirectToPage();

        // accept the invite and add to UserGroup
        invite.IsAccepted = true;
        _context.UserGroup.Add(new UserGroup
        {
            UserId = userId.Value,
            GroupId = invite.GroupId
        });

        await _context.SaveChangesAsync();
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

        // just mark as declined
        invite.IsAccepted = false;
        await _context.SaveChangesAsync();

        return RedirectToPage();
    }

    // === Private helpers ===

    private bool IsStudent(int? userId, string? role) =>
        userId != null && role == "Student";

    private async Task<IList<GroupInvitation>> GetPendingInvitesAsync(int userId)
    {
        return await _context.GroupInvitation
            .Include(i => i.Group)
            .Where(i => i.InvitedUserId == userId && i.IsAccepted == null)
            .ToListAsync();
    }

    private async Task<bool> UserIsInTooManyGroups(int userId)
    {
        int count = await _context.UserGroup.CountAsync(ug => ug.UserId == userId);
        return count >= 3;
    }

    private async Task<GroupInvitation?> GetInviteAsync(int userId)
    {
        return await _context.GroupInvitation
            .Include(i => i.Group)
            .FirstOrDefaultAsync(i =>
                i.InvitationId == ActionInvitationId &&
                i.InvitedUserId == userId &&
                i.IsAccepted == null);
    }
}
