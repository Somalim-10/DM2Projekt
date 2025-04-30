using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using DM2Projekt.Data;
using DM2Projekt.Models;

namespace DM2Projekt.Pages.Groups
{
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

            // Load the group with the associated CreatedByUser for validation in the view
            var group = await _context.Group
                .Include(g => g.CreatedByUser) // Ensure CreatedByUser is loaded
                .FirstOrDefaultAsync(m => m.GroupId == id);

            if (group == null)
                return NotFound();

            var userRole = HttpContext.Session.GetString("UserRole");
            var userId = HttpContext.Session.GetInt32("UserId");

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

            // Fetch the group from the database, including the CreatedByUser to get the CreatedByUserId
            var groupFromDb = await _context.Group
                .Include(g => g.CreatedByUser) // Ensure CreatedByUser is included
                .FirstOrDefaultAsync(g => g.GroupId == Group.GroupId);

            if (groupFromDb == null)
                return NotFound();

            // Now, evaluate if the user is an Admin or the Owner based on the re-fetched group
            var isAdmin = userRole == "Admin";
            var isOwner = groupFromDb.CreatedByUserId == userId; // Now correctly evaluates ownership

            // Check if the user is authorized to edit (either Admin or Owner)
            if (!(isAdmin || isOwner))
                return RedirectToPage("/Groups/Index");

            if (!ModelState.IsValid)
                return Page();

            // Manually preserve the CreatedByUserId to avoid overwriting it during updates
            Group.CreatedByUserId = groupFromDb.CreatedByUserId; // Ensure the CreatedByUserId is not lost

            // Apply the updated values to the existing group entity
            _context.Entry(groupFromDb).CurrentValues.SetValues(Group);

            try
            {
                // Save changes to the database
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!GroupExists(Group.GroupId))
                    return NotFound();
                else
                    throw;
            }

            return RedirectToPage("./Index");
        }

        private bool GroupExists(int id)
        {
            return _context.Group.Any(e => e.GroupId == id);
        }
    }
}
