using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using DM2Projekt.Data;
using DM2Projekt.Models;
using Microsoft.AspNetCore.Mvc;

namespace DM2Projekt.Pages.Groups;

public class IndexModel : PageModel
{
    private readonly DM2ProjektContext _context;

    public IndexModel(DM2ProjektContext context)
    {
        _context = context;
    }

    public IList<Group> Groups { get; set; } = default!;

    [BindProperty(SupportsGet = true)]
    public string? Search { get; set; }

    public int TotalGroupsCount { get; set; } // total groups overall
    public int UserGroupsCount { get; set; }  // groups user belongs to
    public string? UserRole { get; set; }
    public int? UserId { get; set; }

    public async Task OnGetAsync()
    {
        UserRole = HttpContext.Session.GetString("UserRole");
        UserId = HttpContext.Session.GetInt32("UserId");

        // base query with owner + members
        var query = _context.Group
            .Include(g => g.CreatedByUser)
            .Include(g => g.UserGroups)
            .AsQueryable();

        TotalGroupsCount = await query.CountAsync();

        if (UserRole == "Student" && UserId.HasValue)
        {
            UserGroupsCount = await query
                .CountAsync(g => g.CreatedByUserId == UserId.Value || g.UserGroups.Any(ug => ug.UserId == UserId.Value));
        }
        else
        {
            UserGroupsCount = 0;
        }

        if (!string.IsNullOrWhiteSpace(Search))
        {
            query = query.Where(g => EF.Functions.Like(g.GroupName, $"%{Search}%"));
        }

        Groups = await query.OrderBy(g => g.GroupName).ToListAsync();
    }
}
