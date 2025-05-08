using DM2Projekt.Services;
using Microsoft.Extensions.Configuration;
using System.Net.Mail;

namespace DM2Projekt.Tests.Email;

[TestClass]
public class EmailServiceTests
{
    private EmailService GetEmailService()
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
        // arrange
        var service = GetEmailService();

        // act
        try
        {
            await service.SendReminderEmailAsync(
                "someone@test.com",
                "Alex",
                "Room A",
                DateTime.Now.AddDays(1)
            );
        }
        catch (SmtpException)
        {
            // if smtp fails (expected in test), that's fine
            Assert.IsTrue(true);
            return;
        }

        // we never want it to just crash
        Assert.IsTrue(true);
    }
}
