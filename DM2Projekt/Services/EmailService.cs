using System.Net;
using System.Net.Mail;

namespace DM2Projekt.Services;

public class EmailService
{
    private readonly IConfiguration _config;

    public EmailService(IConfiguration config)
    {
        _config = config;
    }

    // sends a reminder email before a booking
    public async Task SendReminderEmailAsync(string toEmail, string firstName, string roomName, DateTime startTime)
    {
        // email setup from appsettings.json
        var smtpHost = _config["Email:SmtpHost"];
        var smtpPort = int.Parse(_config["Email:SmtpPort"]);
        var smtpUser = _config["Email:SmtpUser"];
        var smtpPass = _config["Email:SmtpPass"];
        var fromEmail = _config["Email:FromEmail"];
        var fromName = _config["Email:FromName"];

        // email content
        var subject = "📅 Heads up – you've got a booking soon!";
        var body = $@"
            Hey {firstName},

            Just a quick heads-up: you’ve got a room booking coming up tomorrow.

            🧾 Room: {roomName}
            🕒 Time: {startTime:dddd, MMMM d} at {startTime:HH:mm}

            Don’t be late 😄

            – Zealand Booking Bot";

        // SMTP client setup
        using var client = new SmtpClient(smtpHost, smtpPort)
        {
            Credentials = new NetworkCredential(smtpUser, smtpPass),
            EnableSsl = true
        };

        // build the email message
        using var message = new MailMessage
        {
            From = new MailAddress(fromEmail, fromName),
            Subject = subject,
            Body = body,
            IsBodyHtml = false
        };

        message.To.Add(toEmail);

        // send it out
        await client.SendMailAsync(message);
    }
}
