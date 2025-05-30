using DM2Projekt.Data;
using DM2Projekt.Models;
using DM2Projekt.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;

namespace DM2Projekt.Tests.Email;

[TestClass]
public class BookingReminderServiceTests
{
    [TestMethod]
    public async Task BookingReminder_Should_Set_ReminderSent()
    {
        // Set up a fresh in-memory DB
        var options = new DbContextOptionsBuilder<DM2ProjektContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        using var context = new DM2ProjektContext(options);

        // Add fake user
        var user = new User
        {
            Email = "test@test.com",
            FirstName = "Test",
            LastName = "User",
            Password = "123456",
            Role = Role.Student
        };

        // Add group and put user in it
        var group = new Group
        {
            GroupName = "Test Group",
            UserGroups = { new UserGroup { User = user } }
        };

        // Add booking that starts in 2 hours (so it should trigger)
        context.Booking.Add(new Booking
        {
            Room = new Room { RoomName = "Test Room" },
            Group = group,
            StartTime = DateTime.Now.AddHours(2),
            EndTime = DateTime.Now.AddHours(4),
            ReminderSent = false
        });

        await context.SaveChangesAsync();

        // Fake the email service. don't send anything real
        var emailMock = new Mock<EmailService>(MockBehavior.Strict, (IConfiguration?)null);
        emailMock.Setup(x =>
            x.SendReminderEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>())
        ).Returns(Task.CompletedTask);

        // Set up fake services like ASP.NET would
        var services = new ServiceCollection()
            .AddSingleton(_ => context)
            .AddSingleton(_ => emailMock.Object)
            .BuildServiceProvider();

        // Logger isn’t used, just pass in a mock
        var loggerMock = new Mock<ILogger<BookingReminderService>>();
        var service = new BookingReminderService(services, loggerMock.Object);

        // Run it
        await service.RunReminderCheckAsync();

        // Check that the reminder flag got flipped
        var booking = context.Booking.First();
        Assert.IsTrue(booking.ReminderSent, "ReminderSent should be true after check");
    }
}
