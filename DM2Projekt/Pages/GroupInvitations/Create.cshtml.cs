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

    [TempData]
    public string? SuccessMessage { get; set; }

    public async Task<IActionResult> OnGetAsync(int? groupId)
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null || groupId == null)
            return RedirectToPage("/Login");

        // load group + members
        Group = await _context.Group
            .Include(g => g.UserGroups)
            .FirstOrDefaultAsync(g => g.GroupId == groupId);

        // only group creator can invite
        if (Group == null || Group.CreatedByUserId != userId)
            return RedirectToPage("/Groups/Index");

        // get current members
        var memberIds = Group.UserGroups.Select(ug => ug.UserId).ToList();

        // get pending invitees
        var pendingIds = await _context.GroupInvitation
            .Where(i => i.GroupId == groupId && i.IsAccepted == null)
            .Select(i => i.InvitedUserId)
            .ToListAsync();

        // get all students and filter out those already in or invited
        var eligible = await _context.User
            .Where(u => u.Role == Role.Student)
            .ToListAsync();

        var filtered = eligible
            .Where(u => !memberIds.Contains(u.UserId) && !pendingIds.Contains(u.UserId))
            .OrderBy(u => u.FirstName)
            .ThenBy(u => u.LastName)
            .Select(u => new
            {
                u.UserId,
                Display = $"{u.FirstName} {u.LastName} ({u.Email})"
            })
            .ToList();

        EligibleUsers = new SelectList(filtered, "UserId", "Display");

        // setup default model values
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

        // don’t send same invite twice
        bool alreadyExists = await _context.GroupInvitation.AnyAsync(i =>
            i.GroupId == GroupInvitation.GroupId &&
            i.InvitedUserId == GroupInvitation.InvitedUserId &&
            i.IsAccepted == null);

        if (alreadyExists)
        {
            ModelState.AddModelError(string.Empty, "This user already has a pending invitation.");
            return await OnGetAsync(GroupInvitation.GroupId);
        }

        GroupInvitation.SentAt = DateTime.Now;
        GroupInvitation.IsAccepted = null;

        _context.GroupInvitation.Add(GroupInvitation);
        await _context.SaveChangesAsync();

        SuccessMessage = "Invitation sent successfully!";
        return RedirectToPage("/Groups/Details", new { id = GroupInvitation.GroupId });
    }
}
