using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using DM2Projekt.Data;
using DM2Projekt.Models;

namespace DM2Projekt.Pages.Groups;

public class CreateModel : PageModel
{
    private readonly DM2ProjektContext _context;

    public CreateModel(DM2ProjektContext context)
    {
        _context = context;
    }

    [BindProperty]
    public Group Group { get; set; } = default!;

    public IActionResult OnGet()
    {
        var userRole = HttpContext.Session.GetString("UserRole");

        // only admins and students can see this page
        if (userRole != "Admin" && userRole != "Student")
            return RedirectToPage("/Groups/Index");

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var userRole = HttpContext.Session.GetString("UserRole");

        // block users that are not admin or student
        if (userRole != "Admin" && userRole != "Student")
            return RedirectToPage("/Groups/Index");

        if (!ModelState.IsValid)
            return Page(); // something went wrong

        // get current logged-in user (from session)
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null)
            return RedirectToPage("/Login");

        var user = await _context.User.FindAsync(userId);

        if (user == null)
            return RedirectToPage("/Login");

        // set who created this group
        Group.CreatedByUserId = user.UserId;

        // save group first
        _context.Group.Add(Group);
        await _context.SaveChangesAsync(); // now we get GroupId

        // add user to their own group
        var membership = new UserGroup
        {
            UserId = user.UserId,
            GroupId = Group.GroupId
        };
        _context.UserGroup.Add(membership);

        // save that too
        await _context.SaveChangesAsync();

        return RedirectToPage("./Index");
    }
}
