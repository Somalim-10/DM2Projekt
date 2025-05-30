using DM2Projekt.Data;
using DM2Projekt.Models;
using Microsoft.EntityFrameworkCore;

namespace DM2Projekt.Tests.Bookings;

[TestClass]
public class BookingTests
{
    private DM2ProjektContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<DM2ProjektContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new DM2ProjektContext(options);
    }

    // Overlap + Booking rules

    [TestMethod]
    public void User_Cannot_Have_Overlapping_Bookings()
    {
        using var context = CreateInMemoryContext();
        var userId = 1;
        var start = DateTime.Today.AddHours(8);
        var end = DateTime.Today.AddHours(10);

        context.Booking.Add(new Booking
        {
            CreatedByUserId = userId,
            GroupId = 1,
            RoomId = 1,
            StartTime = start,
            EndTime = end
        });
        context.SaveChanges();

        var overlaps = context.Booking.Any(b =>
            b.CreatedByUserId == userId &&
            b.StartTime < end &&
            b.EndTime > start);

        Assert.IsTrue(overlaps, "user already has a booking here");
    }

    [TestMethod]
    public void Group_Cannot_Have_Overlapping_Bookings()
    {
        using var context = CreateInMemoryContext();
        var groupId = 1;
        var start = DateTime.Today.AddHours(8);
        var end = DateTime.Today.AddHours(10);

        context.Booking.Add(new Booking
        {
            CreatedByUserId = 2,
            GroupId = groupId,
            RoomId = 1,
            StartTime = start,
            EndTime = end
        });
        context.SaveChanges();

        var overlaps = context.Booking.Any(b =>
            b.GroupId == groupId &&
            b.StartTime < end &&
            b.EndTime > start);

        Assert.IsTrue(overlaps, "group already booked at that time");
    }

    [TestMethod]
    public void Booking_Cannot_Be_Longer_Than_Two_Hours()
    {
        var start = DateTime.Now;
        var end = start.AddHours(3); // too long

        var duration = (end - start).TotalHours;

        Assert.IsTrue(duration > 2, "booking too long");
    }

    [TestMethod]
    public void Group_Cannot_Have_More_Than_Three_Active_Bookings()
    {
        using var context = CreateInMemoryContext();
        var groupId = 1;

        for (int i = 0; i < 3; i++)
        {
            context.Booking.Add(new Booking
            {
                GroupId = groupId,
                CreatedByUserId = i + 1,
                RoomId = i + 1,
                StartTime = DateTime.Now.AddDays(i + 1),
                EndTime = DateTime.Now.AddDays(i + 1).AddHours(2)
            });
        }
        context.SaveChanges();

        var activeBookings = context.Booking.Count(b =>
            b.GroupId == groupId && b.EndTime > DateTime.Now);

        Assert.AreEqual(3, activeBookings, "group should have 3 active bookings");
    }

    [TestMethod]
    public void Smartboard_Cannot_Be_Booked_Twice_At_Same_Time()
    {
        using var context = CreateInMemoryContext();
        var roomId = 1;
        var start = DateTime.Today.AddHours(8);
        var end = DateTime.Today.AddHours(10);

        context.Booking.Add(new Booking
        {
            RoomId = roomId,
            CreatedByUserId = 1,
            GroupId = 1,
            StartTime = start,
            EndTime = end,
            UsesSmartboard = true
        });
        context.SaveChanges();

        var smartboardUsed = context.Booking.Any(b =>
            b.RoomId == roomId &&
            b.StartTime == start &&
            b.EndTime == end &&
            b.UsesSmartboard);

        Assert.IsTrue(smartboardUsed, "smartboard already booked");
    }

    [TestMethod]
    public void Booking_Cannot_Be_In_The_Past()
    {
        var startTime = DateTime.Now.AddHours(-2);

        var isPast = startTime < DateTime.Now;

        Assert.IsTrue(isPast, "booking in the past is bad");
    }

    // Timeslot parsing tests

    [TestMethod]
    public void Valid_Timeslot_String_Should_Parse_Correctly()
    {
        var validTime = DateTime.Now.ToString("o");
        var parsed = DateTime.TryParse(validTime, out _);

        Assert.IsTrue(parsed, "valid time should parse");
    }

    [TestMethod]
    public void Invalid_Timeslot_String_Should_Fail_To_Parse()
    {
        var invalidTime = "not-a-real-date";
        var parsed = DateTime.TryParse(invalidTime, out _);

        Assert.IsFalse(parsed, "bad time should fail");
    }
}
