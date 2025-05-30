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

    private SmtpClient CreateSmtpClient()
    {
        // builds the SMTP client using config. nothing fancy
        var smtpHost = _config["Email:SmtpHost"];
        var smtpPort = int.Parse(_config["Email:SmtpPort"]);
        var smtpUser = _config["Email:SmtpUser"];
        var smtpPass = _config["Email:SmtpPass"];

        return new SmtpClient(smtpHost, smtpPort)
        {
            Credentials = new NetworkCredential(smtpUser, smtpPass),
            EnableSsl = true
        };
    }

    private MailMessage CreateMessage(string toEmail, string subject, string body)
    {
        var fromEmail = _config["Email:FromEmail"];
        var fromName = _config["Email:FromName"];

        var message = new MailMessage
        {
            From = new MailAddress(fromEmail, fromName),
            Subject = subject,
            Body = body,
            IsBodyHtml = false
        };

        message.To.Add(toEmail);
        return message;
    }

    private async Task SendEmailAsync(string toEmail, string subject, string body)
    {
        // this is the only place that actually sends emails
        // if stuff breaks, we know exactly where to look
        using var client = CreateSmtpClient();
        using var message = CreateMessage(toEmail, subject, body);
        await client.SendMailAsync(message);
    }

    public virtual Task SendReminderEmailAsync(string toEmail, string firstName, string roomName, DateTime startTime)
    {
        var subject = "📅 Heads up – you've got a booking soon!";
        var body =
$"""
Hey {firstName},

Just a quick heads-up: you’ve got a room booking coming up tomorrow.

📄 Room: {roomName}
🕒 Time: {startTime:dddd, MMMM d} at {startTime:HH:mm}

Don’t be late 😄

– Zealand Booking Bot 👍
""";

        return SendEmailAsync(toEmail, subject, body);
    }

    public Task SendPasswordRecoveryEmailAsync(string toEmail, string firstName, string password)
    {
        var subject = "🔐 Your Zealand Booking Password";
        var body =
$"""
Hey {firstName},

You (or someone pretending to be you 👀) asked to recover the password for your Zealand Booking account.

Here it is:

🔑 Password: {password}

If you didn’t ask for this, just ignore it and move on.

– Zealand Booking Bot 📨
""";

        return SendEmailAsync(toEmail, subject, body);
    }
}
