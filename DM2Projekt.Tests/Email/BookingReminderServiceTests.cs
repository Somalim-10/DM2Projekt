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
        // spin up a fake in-memory db
        var options = new DbContextOptionsBuilder<DM2ProjektContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        using var context = new DM2ProjektContext(options);

        // make a fake user with all required stuff
        var user = new User
        {
            Email = "test@test.com",
            FirstName = "Test",
            LastName = "User",
            Password = "123456",
            Role = Models.Enums.Role.Student
        };

        // make a fake group with user inside
        var group = new Group
        {
            GroupName = "Test Group",
            UserGroups =
            [
                new() { User = user }
            ]
        };

        // add booking with that group and a room
        context.Booking.Add(new Booking
        {
            Room = new Room { RoomName = "Test Room" }, // ✅ room required
            Group = group,
            StartTime = DateTime.Now.AddHours(2),
            EndTime = DateTime.Now.AddHours(4), // add an EndTime just in case
            ReminderSent = false
        });

        context.SaveChanges(); //

        // mock email sending (no real emails please)
        var emailMock = new Mock<EmailService>(MockBehavior.Strict, (IConfiguration?)null);
        emailMock.Setup(x =>
            x.SendReminderEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>())
        ).Returns(Task.CompletedTask);

        // setup services
        var services = new ServiceCollection()
            .AddSingleton(_ => context)
            .AddSingleton(_ => emailMock.Object)
            .BuildServiceProvider();

        var loggerMock = new Mock<ILogger<BookingReminderService>>();
        var service = new BookingReminderService(services, loggerMock.Object);

        // run it once with a short token timeout
        var tokenSource = new CancellationTokenSource();
        tokenSource.CancelAfter(100);

        await service.StartAsync(tokenSource.Token);

        // test passed if this is now true
        var booking = context.Booking.First();
        Assert.IsTrue(booking.ReminderSent);
    }
}
