using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using DM2Projekt.Data;
using DM2Projekt.Models;

namespace DM2Projekt.Pages.GroupInvitations;

public class DeleteModel : PageModel
{
    private readonly DM2ProjektContext _context;

    public DeleteModel(DM2ProjektContext context)
    {
        _context = context;
    }

    [BindProperty]
    public GroupInvitation GroupInvitation { get; set; } = default!;

    public async Task<IActionResult> OnGetAsync(int? id)
    {
        if (id == null)
            return NotFound();

        var invitation = await GetInvitationAsync(id.Value);
        if (invitation == null)
            return NotFound();

        GroupInvitation = invitation;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int? id)
    {
        if (id == null)
            return NotFound();

        var invitation = await _context.GroupInvitation.FindAsync(id);
        if (invitation != null)
        {
            GroupInvitation = invitation;
            _context.GroupInvitation.Remove(invitation);
            await _context.SaveChangesAsync();
        }

        return RedirectToPage("./Index");
    }

    // Pulls the full invitation for display
    private async Task<GroupInvitation?> GetInvitationAsync(int id)
    {
        return await _context.GroupInvitation
            .Include(i => i.Group)
            .Include(i => i.InvitedUser)
            .FirstOrDefaultAsync(i => i.InvitationId == id);
    }
}
