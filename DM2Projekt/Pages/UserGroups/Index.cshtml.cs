using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using DM2Projekt.Data;
using DM2Projekt.Models;
using Microsoft.AspNetCore.Mvc;

namespace DM2Projekt.Pages.UserGroups;

public class IndexModel : PageModel
{
    private readonly DM2ProjektContext _context;

    public IndexModel(DM2ProjektContext context)
    {
        _context = context;
    }

    // yo, this list holds all our user-group links
    public IList<UserGroup> UserGroup { get; set; } = new List<UserGroup>();

    [BindProperty(SupportsGet = true)]
    public string? Search { get; set; }

    public async Task OnGetAsync()
    {
        // grab all the user-group pairs, filter if searching
        var query = _context.UserGroup
            .Include(ug => ug.User)
            .Include(ug => ug.Group)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(Search))
        {
            query = query.Where(ug =>
                ug.User.Email.Contains(Search) ||
                ug.Group.GroupName.Contains(Search));
        }

        UserGroup = await query.ToListAsync();
    }
}
