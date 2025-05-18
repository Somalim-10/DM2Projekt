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
        // 🧪 Set up an in-memory database just for this test
        // super fast, nothing is saved for real
        var options = new DbContextOptionsBuilder<DM2ProjektContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()) // unique name = clean test
            .Options;

        using var context = new DM2ProjektContext(options);

        // 👤 Create a fake user (they'll get the email)
        var user = new User
        {
            Email = "test@test.com",
            FirstName = "Test",
            LastName = "User",
            Password = "123456", // not relevant here
            Role = Role.Student
        };

        // 👥 Create a group and stick that user in it
        var group = new Group
        {
            GroupName = "Test Group",
            UserGroups =
            [
                new() { User = user }
            ]
        };

        // 🏠 Set up a room booking starting in 2 hours — should trigger a reminder
        context.Booking.Add(new Booking
        {
            Room = new Room { RoomName = "Test Room" },
            Group = group,
            StartTime = DateTime.Now.AddHours(2),
            EndTime = DateTime.Now.AddHours(4),
            ReminderSent = false // ✅ we're checking if this gets flipped
        });

        await context.SaveChangesAsync(); // save to the fake db

        // 📧 Create a fake EmailService so we don't actually send emails
        var emailMock = new Mock<EmailService>(MockBehavior.Strict, (IConfiguration?)null);

        // whenever SendReminderEmailAsync gets called, just say "yep, cool"
        emailMock.Setup(x =>
            x.SendReminderEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>())
        ).Returns(Task.CompletedTask);

        // 🧰 Build a fake service container — kinda like how ASP.NET Core would
        var services = new ServiceCollection()
            .AddSingleton(_ => context)
            .AddSingleton(_ => emailMock.Object)
            .BuildServiceProvider();

        // 📒 Logger isn't needed here, just give it a mock
        var loggerMock = new Mock<ILogger<BookingReminderService>>();
        var service = new BookingReminderService(services, loggerMock.Object);

        // 🚀 Run the reminder logic directly — no timers, no waiting
        await service.RunReminderCheckAsync();

        // ✅ Now grab the booking and double-check that the flag got flipped
        var booking = context.Booking.First();
        Assert.IsTrue(booking.ReminderSent);
    }
}
