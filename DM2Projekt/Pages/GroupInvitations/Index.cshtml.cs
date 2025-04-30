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

    public IList<GroupInvitation> GroupInvitation { get; set; } = [];

    [BindProperty]
    public int ActionInvitationId { get; set; } // for Accept/Decline form binding

    public async Task<IActionResult> OnGetAsync()
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        var userRole = HttpContext.Session.GetString("UserRole");

        if (userId == null || userRole != "Student")
            return RedirectToPage("/Index");

        GroupInvitation = await _context.GroupInvitation
            .Include(i => i.Group)
            .Where(i => i.InvitedUserId == userId && i.IsAccepted == null)
            .ToListAsync();

        return Page();
    }

    public async Task<IActionResult> OnPostAcceptAsync()
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null)
            return RedirectToPage("/Login");

        var invite = await _context.GroupInvitation
            .Include(i => i.Group)
            .FirstOrDefaultAsync(i =>
                i.InvitationId == ActionInvitationId &&
                i.InvitedUserId == userId &&
                i.IsAccepted == null);

        if (invite == null)
            return RedirectToPage();

        // Accept the invitation
        invite.IsAccepted = true;

        // Add user to group
        var userGroup = new UserGroup
        {
            UserId = userId.Value,
            GroupId = invite.GroupId
        };
        _context.UserGroup.Add(userGroup);

        await _context.SaveChangesAsync();
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeclineAsync()
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null)
            return RedirectToPage("/Login");

        var invite = await _context.GroupInvitation
            .FirstOrDefaultAsync(i =>
                i.InvitationId == ActionInvitationId &&
                i.InvitedUserId == userId &&
                i.IsAccepted == null);

        if (invite == null)
            return RedirectToPage();

        // Decline the invitation
        invite.IsAccepted = false;

        await _context.SaveChangesAsync();
        return RedirectToPage();
    }
}
