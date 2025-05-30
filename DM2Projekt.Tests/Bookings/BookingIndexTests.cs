using DM2Projekt.Data;
using DM2Projekt.Models;
using Microsoft.EntityFrameworkCore;

namespace DM2Projekt.Tests.Bookings;

[TestClass]
public class BookingIndexTests
{
    private DM2ProjektContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<DM2ProjektContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new DM2ProjektContext(options);
    }

    [TestMethod]
    public async Task Bookings_Filter_By_RoomId_Works()
    {
        using var context = CreateInMemoryContext();

        // Add 2 rooms and 2 bookings
        context.Room.AddRange(
            new Room { RoomId = 1, RoomName = "Room A" },
            new Room { RoomId = 2, RoomName = "Room B" }
        );

        context.Booking.AddRange(
            new Booking
            {
                RoomId = 1,
                GroupId = 1,
                CreatedByUserId = 1,
                StartTime = DateTime.Now.AddHours(1),
                EndTime = DateTime.Now.AddHours(3)
            },
            new Booking
            {
                RoomId = 2,
                GroupId = 2,
                CreatedByUserId = 2,
                StartTime = DateTime.Now.AddHours(2),
                EndTime = DateTime.Now.AddHours(4)
            }
        );

        context.SaveChanges();

        var bookings = context.Booking.Where(b => b.RoomId == 1).ToList();

        Assert.AreEqual(1, bookings.Count, "should only get 1 booking for RoomId 1");
        Assert.AreEqual(1, bookings.First().RoomId);
    }

    [TestMethod]
    public async Task Bookings_Filter_By_Status_Upcoming_Works()
    {
        using var context = CreateInMemoryContext();
        var now = DateTime.Now;

        context.Booking.AddRange(
            new Booking { StartTime = now.AddHours(2), EndTime = now.AddHours(4) },
            new Booking { StartTime = now.AddHours(-3), EndTime = now.AddHours(-1) }
        );
        context.SaveChanges();

        var upcoming = context.Booking.Where(b => b.StartTime > now).ToList();

        Assert.AreEqual(1, upcoming.Count, "should find 1 upcoming booking");
    }

    [TestMethod]
    public async Task Bookings_Filter_By_Status_Ongoing_Works()
    {
        using var context = CreateInMemoryContext();
        var now = DateTime.Now;

        context.Booking.AddRange(
            new Booking { StartTime = now.AddMinutes(-30), EndTime = now.AddMinutes(30) },
            new Booking { StartTime = now.AddHours(2), EndTime = now.AddHours(4) }
        );
        context.SaveChanges();

        var ongoing = context.Booking.Where(b => b.StartTime <= now && b.EndTime > now).ToList();

        Assert.AreEqual(1, ongoing.Count, "should find 1 ongoing booking");
    }

    [TestMethod]
    public async Task Bookings_Filter_By_Status_Past_Works()
    {
        using var context = CreateInMemoryContext();
        var now = DateTime.Now;

        context.Booking.AddRange(
            new Booking { StartTime = now.AddHours(-5), EndTime = now.AddHours(-3) },
            new Booking { StartTime = now.AddHours(1), EndTime = now.AddHours(3) }
        );
        context.SaveChanges();

        var past = context.Booking.Where(b => b.EndTime <= now).ToList();

        Assert.AreEqual(1, past.Count, "should find 1 past booking");
    }
}
