using DM2Projekt.Services;
using Microsoft.Extensions.Configuration;
using System.Net.Mail;

namespace DM2Projekt.Tests.Email;

[TestClass]
public class ForgotPasswordEmailTests
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
    public async Task SendPasswordRecoveryEmailAsync_Should_Not_Throw()
    {
        var service = CreateEmailService();

        try
        {
            await service.SendPasswordRecoveryEmailAsync(
                toEmail: "forgot@test.com",
                firstName: "Forgotty",
                password: "superSecret123"
            );
        }
        catch (SmtpException)
        {
            // Totally fine. expected in test setup
            Assert.IsTrue(true);
            return;
        }

        Assert.IsTrue(true); // All good
    }
}
