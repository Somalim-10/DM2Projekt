using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using DM2Projekt.Data;
using DM2Projekt.Models;

namespace DM2Projekt.Pages.UserGroups;

public class IndexModel : PageModel
{
    private readonly DM2ProjektContext _context;

    public IndexModel(DM2ProjektContext context)
    {
        _context = context;
    }

    public IList<UserGroup> UserGroup { get; set; } = default!;

    public async Task OnGetAsync()
    {
        // load UserGroups including related User and Group info
        UserGroup = await _context.UserGroup
            .Include(u => u.Group)
            .Include(u => u.User)
            .ToListAsync();
    }
}
