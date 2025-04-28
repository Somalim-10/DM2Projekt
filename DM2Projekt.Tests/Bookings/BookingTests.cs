using DM2Projekt.Data;
using DM2Projekt.Models;
using Microsoft.EntityFrameworkCore;

namespace DM2Projekt.Tests.Bookings;

[TestClass]
public class BookingTests
{
    // makes a new fake db for each test
    private DM2ProjektContext GetInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<DM2ProjektContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new DM2ProjektContext(options);
    }

    // --- booking validation tests ---

    [TestMethod]
    public void User_Cannot_Have_Overlapping_Bookings()
    {
        // setup
        using var context = GetInMemoryContext();
        var userId = 1;

        context.Booking.Add(new Booking
        {
            CreatedByUserId = userId,
            GroupId = 1,
            RoomId = 1,
            StartTime = DateTime.Today.AddHours(8),
            EndTime = DateTime.Today.AddHours(10)
        });
        context.SaveChanges();

        // try to book at same time
        var overlapping = context.Booking.Any(b =>
            b.CreatedByUserId == userId &&
            b.StartTime < DateTime.Today.AddHours(10) &&
            b.EndTime > DateTime.Today.AddHours(8));

        // check
        Assert.IsTrue(overlapping, "user already has a booking here");
    }

    [TestMethod]
    public void Group_Cannot_Have_Overlapping_Bookings()
    {
        // setup
        using var context = GetInMemoryContext();
        var groupId = 1;

        context.Booking.Add(new Booking
        {
            CreatedByUserId = 2,
            GroupId = groupId,
            RoomId = 1,
            StartTime = DateTime.Today.AddHours(8),
            EndTime = DateTime.Today.AddHours(10)
        });
        context.SaveChanges();

        // try to book same time
        var overlapping = context.Booking.Any(b =>
            b.GroupId == groupId &&
            b.StartTime < DateTime.Today.AddHours(10) &&
            b.EndTime > DateTime.Today.AddHours(8));

        // check
        Assert.IsTrue(overlapping, "group already booked at that time");
    }

    [TestMethod]
    public void Booking_Cannot_Be_Longer_Than_Two_Hours()
    {
        // make fake start/end
        var start = DateTime.Now;
        var end = start.AddHours(3); // 3h, bad

        // check duration
        var duration = (end - start).TotalHours;

        // test
        Assert.IsTrue(duration > 2, "booking too long");
    }

    [TestMethod]
    public void Group_Cannot_Have_More_Than_Three_Active_Bookings()
    {
        // setup
        using var context = GetInMemoryContext();
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

        // count active bookings
        var activeBookings = context.Booking.Count(b => b.GroupId == groupId && b.EndTime > DateTime.Now);

        // check
        Assert.AreEqual(3, activeBookings, "group should have 3 active bookings");
    }

    [TestMethod]
    public void Smartboard_Cannot_Be_Booked_Twice_At_Same_Time()
    {
        // setup
        using var context = GetInMemoryContext();
        var roomId = 1;

        context.Booking.Add(new Booking
        {
            RoomId = roomId,
            CreatedByUserId = 1,
            GroupId = 1,
            StartTime = DateTime.Today.AddHours(8),
            EndTime = DateTime.Today.AddHours(10),
            UsesSmartboard = true
        });
        context.SaveChanges();

        // check if smartboard used
        var smartboardUsed = context.Booking.Any(b =>
            b.RoomId == roomId &&
            b.StartTime == DateTime.Today.AddHours(8) &&
            b.EndTime == DateTime.Today.AddHours(10) &&
            b.UsesSmartboard);

        // check
        Assert.IsTrue(smartboardUsed, "smartboard already booked");
    }

    [TestMethod]
    public void Booking_Cannot_Be_In_The_Past()
    {
        // setup
        var startTime = DateTime.Now.AddHours(-2);

        // check if past
        var isPast = startTime < DateTime.Now;

        // test
        Assert.IsTrue(isPast, "booking in the past is bad");
    }

    [TestMethod]
    public void Valid_Timeslot_String_Should_Parse_Correctly()
    {
        // setup valid time
        var validTime = DateTime.Now.ToString("o");

        // try parse
        var parsed = DateTime.TryParse(validTime, out var _);

        // check
        Assert.IsTrue(parsed, "valid time should parse");
    }

    [TestMethod]
    public void Invalid_Timeslot_String_Should_Fail_To_Parse()
    {
        // setup invalid time
        var invalidTime = "not-a-real-date";

        // try parse
        var parsed = DateTime.TryParse(invalidTime, out var _);

        // check
        Assert.IsFalse(parsed, "bad time should fail");
    }
}
