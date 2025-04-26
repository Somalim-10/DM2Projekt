using DM2Projekt.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace DM2Projekt.Pages;

public class LoginModel : PageModel
{
    private readonly DM2ProjektContext _context;

    public LoginModel(DM2ProjektContext context)
    {
        _context = context;
    }

    [BindProperty]
    public LoginInputModel Input { get; set; } // form inputs

    public string ErrorMessage { get; set; } // error text

    public void OnGet()
    {
        // just show page
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
            return Page(); // form not ok

        var user = await _context.User
            .FirstOrDefaultAsync(u => u.Email == Input.Email && u.Password == Input.Password);

        if (user == null)
        {
            ErrorMessage = "Invalid email or password.";
            return Page(); // wrong login
        }

        // save user info in session
        HttpContext.Session.SetInt32("UserId", user.UserId);
        HttpContext.Session.SetString("UserName", $"{user.FirstName} {user.LastName}");

        return RedirectToPage("/Index"); // go home
    }

    // this gets filled with stuff from the login form when user submits
    public class LoginInputModel
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }
}
