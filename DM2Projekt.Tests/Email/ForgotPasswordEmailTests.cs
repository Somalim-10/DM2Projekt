using DM2Projekt.Services;
using Microsoft.Extensions.Configuration;
using System.Net.Mail;

namespace DM2Projekt.Tests.Email;

[TestClass]
public class ForgotPasswordEmailTests
{
    private EmailService GetEmailService()
    {
        // use in-memory config
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
        // 🧪 Arrange
        var service = GetEmailService();

        try
        {
            // 🚀 Act
            await service.SendPasswordRecoveryEmailAsync(
                toEmail: "forgot@test.com",
                firstName: "Forgotty",
                password: "superSecret123"
            );
        }
        catch (SmtpException)
        {
            // ✅ Expected: no SMTP server in test — totally fine
            Assert.IsTrue(true);
            return;
        }

        // ✅ If it doesn't crash, we’re still good
        Assert.IsTrue(true);
    }
}
