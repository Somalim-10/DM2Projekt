using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using DM2Projekt.Data;
using DM2Projekt.Models;

namespace DM2Projekt.Pages.Groups;

public class DeleteModel : PageModel
{
    private readonly DM2ProjektContext _context;

    public DeleteModel(DM2ProjektContext context)
    {
        _context = context;
    }

    [BindProperty]
    public Group Group { get; set; } = default!;

    public async Task<IActionResult> OnGetAsync(int? id)
    {
        if (id == null)
            return NotFound();

        // grab the group with its creator for display
        var group = await _context.Group
            .Include(g => g.CreatedByUser)
            .FirstOrDefaultAsync(g => g.GroupId == id);

        if (group == null)
            return NotFound();

        var userId = HttpContext.Session.GetInt32("UserId");
        var userRole = HttpContext.Session.GetString("UserRole");

        // 🛑 Only admins or the person who made it can delete
        if (userRole != "Admin" && group.CreatedByUserId != userId)
            return RedirectToPage("/Groups/Index");

        Group = group;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int? id)
    {
        if (id == null)
            return NotFound();

        var group = await _context.Group.FindAsync(id);
        if (group == null)
            return NotFound();

        var userId = HttpContext.Session.GetInt32("UserId");
        var userRole = HttpContext.Session.GetString("UserRole");

        // check again just to be safe
        if (userRole != "Admin" && group.CreatedByUserId != userId)
            return RedirectToPage("/Groups/Index");

        // ✅ Cleanly delete everything related to this group
        await DeleteGroupAndChildren(group.GroupId);

        return RedirectToPage("./Index");
    }

    /// <summary>
    /// 🔨 Deletes a group and all its attached data:
    /// - bookings
    /// - user memberships
    /// - invitations
    /// </summary>
    private async Task DeleteGroupAndChildren(int groupId)
    {
        // get the group and all its related stuff
        var group = await _context.Group
            .Include(g => g.Bookings)
            .Include(g => g.UserGroups)
            .FirstOrDefaultAsync(g => g.GroupId == groupId);

        if (group == null)
            return;

        // 🚫 Delete bookings first (otherwise they'll block the group from being deleted)
        if (group.Bookings.Any())
            _context.Booking.RemoveRange(group.Bookings);

        // 🚫 Then user-group links (memberships)
        if (group.UserGroups.Any())
            _context.UserGroup.RemoveRange(group.UserGroups);

        // 🚫 Then any pending invitations
        var invites = await _context.GroupInvitation
            .Where(i => i.GroupId == groupId)
            .ToListAsync();

        if (invites.Any())
            _context.GroupInvitation.RemoveRange(invites);

        // 🎯 Finally, remove the group itself
        _context.Group.Remove(group);

        await _context.SaveChangesAsync();
    }
}
