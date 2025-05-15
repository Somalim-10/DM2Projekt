using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using DM2Projekt.Data;
using DM2Projekt.Models;

namespace DM2Projekt.Pages.Groups;

public class DeleteModel : PageModel
{
    private readonly DM2ProjektContext _context;
    // Constructor: injects the database context so we can access the database
    public DeleteModel(DM2ProjektContext context)
    {
        _context = context;
    }

    [BindProperty]
    // This property is bound to the form on the page. It holds the group being deleted.
    public Group Group { get; set; } = default!;

    // GET: Called when the user navigates to the delete page (usually to confirm deletion)
    public async Task<IActionResult> OnGetAsync(int? id)
    {
        //If the ID doesn't exist, return error
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
    // POST: Called when the user confirms the deletion and submits the form
    public async Task<IActionResult> OnPostAsync(int? id)
    {
        // If no ID is provided, return error
        if (id == null)
            return NotFound();
        // Try to find the group in the database
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
