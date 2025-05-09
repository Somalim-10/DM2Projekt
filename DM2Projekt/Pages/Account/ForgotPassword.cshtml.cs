using DM2Projekt.Data;
using DM2Projekt.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace DM2Projekt.Pages.Account;

/// <summary>
/// Handles "forgot password" page logic. User types in email, we send them their password (step 2).
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
        // if they somehow bypass the frontend validation — double check
        if (!ModelState.IsValid)
            return Page();

        // we're not doing anything just yet, that’s next step
        Message = "Hang tight — sending feature coming in the next step.";
        return Page();
    }
}
