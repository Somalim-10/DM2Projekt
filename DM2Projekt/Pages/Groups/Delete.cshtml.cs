using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using DM2Projekt.Data;
using DM2Projekt.Models;

namespace DM2Projekt.Pages.Groups;

public class DeleteModel : PageModel
{
    private readonly DM2ProjektContext _context;

    // inject DB context
    public DeleteModel(DM2ProjektContext context)
    {
        _context = context;
    }

    // bound property for group to delete
    [BindProperty]
    public Group Group { get; set; } = default!;

    // GET: load group details & check permissions
    public async Task<IActionResult> OnGetAsync(int? id)
    {
        if (id == null) return NotFound();

        var group = await _context.Group
            .Include(g => g.CreatedByUser)
            .FirstOrDefaultAsync(g => g.GroupId == id);

        if (group == null) return NotFound();

        var userId = HttpContext.Session.GetInt32("UserId");
        var userRole = HttpContext.Session.GetString("UserRole");

        // only admin or owner allowed to delete
        if (userRole != "Admin" && group.CreatedByUserId != userId)
            return RedirectToPage("/Groups/Index");

        Group = group;
        return Page();
    }

    // POST: confirm delete, remove everything related
    public async Task<IActionResult> OnPostAsync(int? id)
    {
        if (id == null) return NotFound();

        var group = await _context.Group.FindAsync(id);
        if (group == null) return NotFound();

        var userId = HttpContext.Session.GetInt32("UserId");
        var userRole = HttpContext.Session.GetString("UserRole");

        // safety check again
        if (userRole != "Admin" && group.CreatedByUserId != userId)
            return RedirectToPage("/Groups/Index");

        // delete group and all related entities
        await DeleteGroupAndChildren(group.GroupId);

        return RedirectToPage("./Index");
    }

    // delete group plus bookings, memberships, invites
    private async Task DeleteGroupAndChildren(int groupId)
    {
        var group = await _context.Group
            .Include(g => g.Bookings)
            .Include(g => g.UserGroups)
            .FirstOrDefaultAsync(g => g.GroupId == groupId);

        if (group == null) return;

        if (group.Bookings.Any())
            _context.Booking.RemoveRange(group.Bookings);

        if (group.UserGroups.Any())
            _context.UserGroup.RemoveRange(group.UserGroups);

        var invites = await _context.GroupInvitation
            .Where(i => i.GroupId == groupId)
            .ToListAsync();

        if (invites.Any())
            _context.GroupInvitation.RemoveRange(invites);

        _context.Group.Remove(group);

        await _context.SaveChangesAsync();
    }
}
