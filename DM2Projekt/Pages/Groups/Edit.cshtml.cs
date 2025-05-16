using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using DM2Projekt.Data;
using DM2Projekt.Models;

namespace DM2Projekt.Pages.Groups;

public class EditModel : PageModel
{
    private readonly DM2ProjektContext _context;

    public EditModel(DM2ProjektContext context)
    {
        _context = context;
    }

    [BindProperty]
    public Group Group { get; set; } = default!;

    public async Task<IActionResult> OnGetAsync(int? id)
    {
        if (id == null)
            return NotFound();

        // grab the group and the user who created it
        var group = await _context.Group
            .Include(g => g.CreatedByUser)
            .FirstOrDefaultAsync(m => m.GroupId == id);

        if (group == null)
            return NotFound();

        var userRole = HttpContext.Session.GetString("UserRole");
        var userId = HttpContext.Session.GetInt32("UserId");

        // only owner or admin can edit
        var isAdmin = userRole == "Admin";
        var isOwner = group.CreatedByUserId == userId;

        if (!(isAdmin || isOwner))
            return RedirectToPage("/Groups/Index");

        Group = group;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var userRole = HttpContext.Session.GetString("UserRole");
        var userId = HttpContext.Session.GetInt32("UserId");

        // double-check the group still exists (and grab the owner again)
        var groupFromDb = await _context.Group
            .Include(g => g.CreatedByUser)
            .FirstOrDefaultAsync(g => g.GroupId == Group.GroupId);

        if (groupFromDb == null)
            return NotFound();

        var isAdmin = userRole == "Admin";
        var isOwner = groupFromDb.CreatedByUserId == userId;

        if (!(isAdmin || isOwner))
            return RedirectToPage("/Groups/Index");

        if (!ModelState.IsValid)
            return Page();

        // don’t mess with the original creator
        Group.CreatedByUserId = groupFromDb.CreatedByUserId;

        // slap the new values onto the old object
        _context.Entry(groupFromDb).CurrentValues.SetValues(Group);

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!GroupExists(Group.GroupId))
                return NotFound();

            throw;
        }

        return RedirectToPage("./Index");
    }

    private bool GroupExists(int id)
    {
        return _context.Group.Any(e => e.GroupId == id);
    }
}
