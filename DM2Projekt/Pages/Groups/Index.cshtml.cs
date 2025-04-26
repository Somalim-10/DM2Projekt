using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using DM2Projekt.Data;
using DM2Projekt.Models;

namespace DM2Projekt.Pages.Groups;

public class IndexModel : PageModel
{
    private readonly DM2ProjektContext _context;

    public IndexModel(DM2ProjektContext context)
    {
        _context = context;
    }

    public IList<Group> Group { get; set; } = default!;

    public async Task OnGetAsync()
    {
        // just load all groups
        Group = await _context.Group.ToListAsync();
    }
}
