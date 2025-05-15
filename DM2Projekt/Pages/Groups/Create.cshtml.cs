using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using DM2Projekt.Data;
using DM2Projekt.Models;
using Microsoft.EntityFrameworkCore;

namespace DM2Projekt.Pages.Groups;

public class CreateModel : PageModel
{
    private readonly DM2ProjektContext _context;

    // Constructor: injects the database context so we can access the database
    public CreateModel(DM2ProjektContext context)
    {
        _context = context;
    }

    [BindProperty]
    // This property is bound to the form on the page. It holds the group being deleted.
    public Group Group { get; set; } = default!;

    public IActionResult OnGet()
    {
        // Get the current user's role from the session
        var userRole = HttpContext.Session.GetString("UserRole");

        // only Admins and Students can make groups
        if (userRole != "Admin" && userRole != "Student")
            return RedirectToPage("/Groups/Index");

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var userRole = HttpContext.Session.GetString("UserRole");

        if (userRole != "Admin" && userRole != "Student")
            return RedirectToPage("/Groups/Index");
        // If form validation fails, redisplay the page with validation messages
        if (!ModelState.IsValid)
            return Page();
        // Get the user ID from session, and If no user ID in session, redirect to login
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null)
            return RedirectToPage("/Login");

        // Try to find the user in the database
        var user = await _context.User.FindAsync(userId);
        // If the user doesn't exist, redirect to login
        if (user == null)
            return RedirectToPage("/Login");

        // Can only create 1 group
        bool alreadyCreated = await _context.Group.AnyAsync(g => g.CreatedByUserId == userId);
        if (alreadyCreated)
        {
            ModelState.AddModelError(string.Empty, "You can only create one group.");
            return Page();
        }

        // Max 3 groups total (creator included)
        int groupCount = await _context.UserGroup.CountAsync(ug => ug.UserId == userId);
        if (groupCount >= 3)
        {
            ModelState.AddModelError(string.Empty, "You cannot be in more than 3 groups.");
            return Page();
        }

        // all good, save group
        Group.CreatedByUserId = user.UserId;
        _context.Group.Add(Group);
        await _context.SaveChangesAsync();

        // add the user as a member of their group
        _context.UserGroup.Add(new UserGroup
        {
            UserId = user.UserId,
            GroupId = Group.GroupId
        });
        await _context.SaveChangesAsync();

        return RedirectToPage("./Index");
    }
}
