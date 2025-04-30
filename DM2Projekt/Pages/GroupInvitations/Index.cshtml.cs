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

    // List of pending invites for the current student
    public IList<GroupInvitation> GroupInvitation { get; set; } = [];

    // Used to bind Accept/Decline forms
    [BindProperty]
    public int ActionInvitationId { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        var userRole = HttpContext.Session.GetString("UserRole");

        // only students allowed here
        if (userId == null || userRole != "Student")
            return RedirectToPage("/Index");

        // grab all pending invites for this student
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

        // check how many groups this user is already in
        int groupCount = await _context.UserGroup.CountAsync(ug => ug.UserId == userId);
        if (groupCount >= 3)
        {
            // add an error so user knows why it failed
            ModelState.AddModelError(string.Empty, "You cannot join more than 3 groups.");

            // re-fetch invites so page doesn't break
            GroupInvitation = await _context.GroupInvitation
                .Include(i => i.Group)
                .Where(i => i.InvitedUserId == userId && i.IsAccepted == null)
                .ToListAsync();

            return Page();
        }

        // find the invite again
        var invite = await _context.GroupInvitation
            .Include(i => i.Group)
            .FirstOrDefaultAsync(i =>
                i.InvitationId == ActionInvitationId &&
                i.InvitedUserId == userId &&
                i.IsAccepted == null);

        if (invite == null)
            return RedirectToPage();

        // accept it and add the user to the group
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

        var invite = await _context.GroupInvitation
            .FirstOrDefaultAsync(i =>
                i.InvitationId == ActionInvitationId &&
                i.InvitedUserId == userId &&
                i.IsAccepted == null);

        if (invite == null)
            return RedirectToPage();

        // just mark it declined
        invite.IsAccepted = false;

        await _context.SaveChangesAsync();
        return RedirectToPage();
    }
}
