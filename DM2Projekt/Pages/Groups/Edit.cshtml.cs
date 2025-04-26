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
        // check if logged in
        var userRole = HttpContext.Session.GetString("UserRole");
        if (userRole != "Admin")
        {
            return RedirectToPage("/Groups/Index");
        }

        if (id == null)
        {
            return NotFound();
        }

        var group = await _context.Group.FirstOrDefaultAsync(m => m.GroupId == id);
        if (group == null)
        {
            return NotFound();
        }

        Group = group;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        // check if logged in
        var userRole = HttpContext.Session.GetString("UserRole");
        if (userRole != "Admin")
        {
            return RedirectToPage("/Groups/Index");
        }

        if (!ModelState.IsValid)
        {
            return Page();
        }

        _context.Attach(Group).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!GroupExists(Group.GroupId))
            {
                return NotFound();
            }
            else
            {
                throw;
            }
        }

        return RedirectToPage("./Index");
    }

    private bool GroupExists(int id)
    {
        return _context.Group.Any(e => e.GroupId == id);
    }
}
