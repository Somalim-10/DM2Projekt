using DM2Projekt.Data;
using DM2Projekt.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace DM2Projekt.Pages.Account;

/// <summary>
/// Handles "forgot password" page logic. User types in email, we send them their password.
/// </summary>
public class ForgotPasswordModel : PageModel
{
    private readonly DM2ProjektContext _context;
    private readonly EmailService _emailService;

    public ForgotPasswordModel(DM2ProjektContext context, EmailService emailService)
    {
        _context = context;
        _emailService = emailService;
    }

    // user input — just the email, nothing fancy
    [BindProperty]
    public ForgotPasswordInputModel Input { get; set; } = new();

    // used to show success/failure message
    public string? Message { get; set; }

    // lil' helper model for the form
    public class ForgotPasswordInputModel
    {
        [Required(ErrorMessage = "You gotta type something here.")]
        [EmailAddress(ErrorMessage = "That doesn’t look like a real email.")]
        public string Email { get; set; }
    }

    // this runs when they click the button
    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
            return Page(); // something's wrong with the input — don't continue

        // see if we have a user with this email
        var user = await _context.User.FirstOrDefaultAsync(u => u.Email == Input.Email);

        if (user != null)
        {
            // send them their password by email
            await _emailService.SendPasswordRecoveryEmailAsync(
                toEmail: user.Email,
                firstName: user.FirstName,
                password: user.Password
            );
        }

        // always show a friendly message — even if the email wasn’t in the system
        Message = "If that email exists in our system, we just sent the password your way. 📬";

        return Page(); // stay on the same page so we can show the message
    }

}
