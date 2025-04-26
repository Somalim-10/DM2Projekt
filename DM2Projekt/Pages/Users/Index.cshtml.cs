using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using DM2Projekt.Data;
using DM2Projekt.Models;

namespace DM2Projekt.Pages.Users;

public class IndexModel : PageModel
{
    private readonly DM2ProjektContext _context;

    public IndexModel(DM2ProjektContext context)
    {
        _context = context;
    }

    public IList<User> User { get; set; } = default!;

    public async Task<IActionResult> OnGetAsync()
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        var userRole = HttpContext.Session.GetString("UserRole");

        if (userId == null) // not logged in
        {
            return RedirectToPage("/Login");
        }

        if (userRole != "Admin") // only Admin can view
        {
            return RedirectToPage("/Index");
        }

        // load all users
        User = await _context.User.ToListAsync();
        return Page();
    }
}
