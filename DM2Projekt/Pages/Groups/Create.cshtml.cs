using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using DM2Projekt.Data;
using DM2Projekt.Models;
using Microsoft.EntityFrameworkCore;

namespace DM2Projekt.Pages.Groups;

public class CreateModel : PageModel
{
    private readonly DM2ProjektContext _context;

    public CreateModel(DM2ProjektContext context)
    {
        _context = context;
    }

    [BindProperty]
    // This holds the group info from the form
    public Group Group { get; set; } = default!;

    public IActionResult OnGet()
    {
        // Only Admins and Students can make groups, others get bounced
        var userRole = HttpContext.Session.GetString("UserRole");
        if (userRole != "Admin" && userRole != "Student")
            return RedirectToPage("/Groups/Index");
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        // Double-check permissions on POST
        var userRole = HttpContext.Session.GetString("UserRole");
        if (userRole != "Admin" && userRole != "Student")
            return RedirectToPage("/Groups/Index");

        // Validate the form inputs
        if (!ModelState.IsValid)
            return Page();

        // Get current user ID from session
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null)
            return RedirectToPage("/Login");

        // Find user in DB, just in case
        var user = await _context.User.FindAsync(userId);
        if (user == null)
            return RedirectToPage("/Login");

        // User can only create one group max
        bool alreadyCreated = await _context.Group.AnyAsync(g => g.CreatedByUserId == userId);
        if (alreadyCreated)
        {
            ModelState.AddModelError(string.Empty, "You can only create one group.");
            return Page();
        }

        // Max 3 groups per user total (including the one they create)
        int groupCount = await _context.UserGroup.CountAsync(ug => ug.UserId == userId);
        if (groupCount >= 3)
        {
            ModelState.AddModelError(string.Empty, "You can’t be in more than 3 groups.");
            return Page();
        }

        // assign ownership and save group
        Group.CreatedByUserId = user.UserId;
        _context.Group.Add(Group);
        await _context.SaveChangesAsync();

        // Add creator as a member of their own group
        _context.UserGroup.Add(new UserGroup
        {
            UserId = user.UserId,
            GroupId = Group.GroupId
        });
        await _context.SaveChangesAsync();

        return RedirectToPage("./Index");
    }
}
