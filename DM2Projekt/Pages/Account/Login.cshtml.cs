using DM2Projekt.Data;
using DM2Projekt.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace DM2Projekt.Pages.Account;

public class LoginModel : PageModel
{
    private readonly DM2ProjektContext _context;

    public LoginModel(DM2ProjektContext context)
    {
        _context = context;
    }

    [BindProperty]
    public LoginInputModel Input { get; set; }

    public string ErrorMessage { get; set; }

    public void OnGet()
    {
        // Nothing to do here — just loading the page
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
            return Page();

        var user = await GetUserAsync(Input.Email, Input.Password);

        if (user == null)
        {
            ErrorMessage = "Invalid email or password.";
            return Page();
        }

        SetUserSession(user);

        return RedirectToPage("/Index");
    }

    // === Private helpers ===

    private async Task<User?> GetUserAsync(string email, string password)
    {
        return await _context.User
            .FirstOrDefaultAsync(u => u.Email == email && u.Password == password);
    }

    private void SetUserSession(User user)
    {
        HttpContext.Session.SetInt32("UserId", user.UserId);
        HttpContext.Session.SetString("UserName", $"{user.FirstName} {user.LastName}");
        HttpContext.Session.SetString("UserRole", user.Role.ToString());
    }

    public class LoginInputModel
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }
}
