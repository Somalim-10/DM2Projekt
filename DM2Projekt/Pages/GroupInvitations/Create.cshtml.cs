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

        // grab the group + members
        Group = await _context.Group
            .Include(g => g.UserGroups)
            .FirstOrDefaultAsync(g => g.GroupId == groupId);

        // only the creator can invite
        if (Group == null || Group.CreatedByUserId != userId)
            return RedirectToPage("/Groups/Index");

        // collect existing members and pending invites
        var memberIds = Group.UserGroups.Select(ug => ug.UserId).ToList();
        var pendingIds = await _context.GroupInvitation
            .Where(i => i.GroupId == groupId && i.IsAccepted == null)
            .Select(i => i.InvitedUserId)
            .ToListAsync();

        // grab all students and filter locally
        var students = await _context.User
            .Where(u => u.Role == Role.Student)
            .ToListAsync();

        var eligible = students
            .Where(u => !memberIds.Contains(u.UserId) && !pendingIds.Contains(u.UserId))
            .ToList();

        EligibleUsers = new SelectList(eligible, "UserId", "Email");

        // pre-fill form data
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

        // don’t send duplicate invite
        bool alreadyExists = await _context.GroupInvitation
            .AnyAsync(i =>
                i.GroupId == GroupInvitation.GroupId &&
                i.InvitedUserId == GroupInvitation.InvitedUserId &&
                i.IsAccepted == null);

        if (alreadyExists)
        {
            ModelState.AddModelError(string.Empty, "This user already has a pending invitation.");
            return await OnGetAsync(GroupInvitation.GroupId); // re-render form
        }

        // save it
        GroupInvitation.SentAt = DateTime.Now;
        GroupInvitation.IsAccepted = null;

        _context.GroupInvitation.Add(GroupInvitation);
        await _context.SaveChangesAsync();

        return RedirectToPage("/Groups/Details", new { id = GroupInvitation.GroupId });
    }
}
