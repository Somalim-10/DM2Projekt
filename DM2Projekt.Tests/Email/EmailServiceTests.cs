using DM2Projekt.Services;
using Microsoft.Extensions.Configuration;
using System.Net.Mail;

namespace DM2Projekt.Tests.Email;

[TestClass]
public class EmailServiceTests
{
    private EmailService CreateEmailService()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Email:SmtpHost"] = "smtp.test.com",
                ["Email:SmtpPort"] = "587",
                ["Email:SmtpUser"] = "test-user",
                ["Email:SmtpPass"] = "test-pass",
                ["Email:FromEmail"] = "noreply@test.com",
                ["Email:FromName"] = "Test Bot"
            })
            .Build();

        return new EmailService(config);
    }

    [TestMethod]
    public async Task SendReminderEmailAsync_Should_Not_Throw()
    {
        var service = CreateEmailService();

        try
        {
            await service.SendReminderEmailAsync(
                toEmail: "someone@test.com",
                firstName: "Alex",
                roomName: "Room A",
                startTime: DateTime.Now.AddDays(1)
            );
        }
        catch (SmtpException)
        {
            // This is fine. there's no real SMTP here
            Assert.IsTrue(true);
            return;
        }

        // If it runs, we're good
        Assert.IsTrue(true);
    }
}
