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

        var group = await _context.Group
            .Include(g => g.CreatedByUser)
            .FirstOrDefaultAsync(m => m.GroupId == id);

        if (group == null)
            return NotFound();

        var userRole = HttpContext.Session.GetString("UserRole");
        var userId = HttpContext.Session.GetInt32("UserId");

        // only admins or the student who created the group
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

        var userRole = HttpContext.Session.GetString("UserRole");
        var userId = HttpContext.Session.GetInt32("UserId");

        // same check again before deletion
        if (userRole != "Admin" && group.CreatedByUserId != userId)
            return RedirectToPage("/Groups/Index");

        _context.Group.Remove(group);
        await _context.SaveChangesAsync();

        return RedirectToPage("./Index");
    }
}
