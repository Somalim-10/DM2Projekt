using System.Net;
using System.Net.Mail;

namespace DM2Projekt.Services;

public class EmailService
{
    private readonly IConfiguration _configuration;

    public EmailService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    // this lil guy sends a reminder email before the booking
    public async Task SendReminderEmailAsync(string toEmail, string firstName, string roomName, DateTime startTime)
    {
        // grab our email setup from appsettings.json (you know, the secret sauce)
        var smtpHost = _configuration["Email:SmtpHost"];
        var smtpPort = int.Parse(_configuration["Email:SmtpPort"]);
        var smtpUser = _configuration["Email:SmtpUser"];
        var smtpPass = _configuration["Email:SmtpPass"];
        var fromEmail = _configuration["Email:FromEmail"];
        var fromName = _configuration["Email:FromName"];

        // build the email content – keep it short and sweet
        var subject = "📅 Heads up – you've got a booking soon!";
        var body = $@"
                    Hey {firstName},
                    
                    Just a quick heads-up: you’ve got a room booking coming up tomorrow.
                    
                    🧾 Room: {roomName}
                    🕒 Time: {startTime:dddd, MMMM d} at {startTime:HH:mm}
                    
                    Don’t be late 😄
                    
                    – Zealand Booking Bot";

        // set up the mail truck
        using var client = new SmtpClient(smtpHost, smtpPort)
        {
            Credentials = new NetworkCredential(smtpUser, smtpPass),
            EnableSsl = true // gotta keep it safe
        };

        // put together the letter we're sending
        using var message = new MailMessage
        {
            From = new MailAddress(fromEmail, fromName),
            Subject = subject,
            Body = body,
            IsBodyHtml = false // plain text = no fuss
        };

        message.To.Add(toEmail); // who's getting the email

        await client.SendMailAsync(message); // and... send it! 🚀
    }
}
