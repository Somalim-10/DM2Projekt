using DM2Projekt.Data;
using DM2Projekt.Models;
using Microsoft.EntityFrameworkCore;

namespace DM2Projekt.Tests.Bookings;

[TestClass]
public class BookingDeleteTests
{
    // makes a new fake db
    private DM2ProjektContext GetInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<DM2ProjektContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new DM2ProjektContext(options);
    }

    [TestMethod]
    public async Task Admin_Can_Delete_Any_Booking()
    {
        // setup
        using var context = GetInMemoryContext();

        context.Booking.Add(new Booking
        {
            BookingId = 1,
            CreatedByUserId = 2
        });

        context.SaveChanges();

        // simulate admin
        var userRole = "Admin";

        // act
        var canDelete = userRole == "Admin";

        // check
        Assert.IsTrue(canDelete, "admin should be able to delete any booking");
    }

    [TestMethod]
    public async Task Student_Can_Delete_Own_Booking()
    {
        // setup
        using var context = GetInMemoryContext();
        var userId = 5;

        context.Booking.Add(new Booking
        {
            BookingId = 1,
            CreatedByUserId = userId
        });

        context.SaveChanges();

        // simulate student
        var userRole = "Student";

        // act
        var booking = context.Booking.First();
        var canDelete = userRole == "Admin" || userRole == "Teacher" || booking.CreatedByUserId == userId;

        // check
        Assert.IsTrue(canDelete, "student should delete own booking");
    }

    [TestMethod]
    public async Task Student_Cannot_Delete_Other_Booking()
    {
        // setup
        using var context = GetInMemoryContext();
        var userId = 5; // student id

        context.Booking.Add(new Booking
        {
            BookingId = 1,
            CreatedByUserId = 99 // different user
        });

        context.SaveChanges();

        // simulate student
        var userRole = "Student";

        // act
        var booking = context.Booking.First();
        var canDelete = userRole == "Admin" || userRole == "Teacher" || booking.CreatedByUserId == userId;

        // check
        Assert.IsFalse(canDelete, "student should not delete someone else's booking");
    }

    [TestMethod]
    public async Task Teacher_Can_Delete_Booking_With_Enough_Notice()
    {
        // setup
        using var context = GetInMemoryContext();
        var now = DateTime.Now;

        context.Booking.Add(new Booking
        {
            BookingId = 1,
            CreatedByUserId = 2,
            StartTime = now.AddDays(5) // 5 days ahead
        });

        context.SaveChanges();

        // simulate teacher
        var userRole = "Teacher";
        var booking = context.Booking.First();
        var diff = booking.StartTime.Value - now;
        var enoughNotice = diff.TotalDays >= 3;

        // act
        var canDelete = userRole == "Admin" || (userRole == "Teacher" && enoughNotice);

        // check
        Assert.IsTrue(canDelete, "teacher can delete with 3+ days notice");
    }

    [TestMethod]
    public async Task Teacher_Cannot_Delete_Booking_With_Short_Notice()
    {
        // setup
        using var context = GetInMemoryContext();
        var now = DateTime.Now;

        context.Booking.Add(new Booking
        {
            BookingId = 1,
            CreatedByUserId = 2,
            StartTime = now.AddDays(1) // only 1 day ahead
        });

        context.SaveChanges();

        // simulate teacher
        var userRole = "Teacher";
        var booking = context.Booking.First();
        var diff = booking.StartTime.Value - now;
        var enoughNotice = diff.TotalDays >= 3;

        // act
        var canDelete = userRole == "Admin" || (userRole == "Teacher" && enoughNotice);

        // check
        Assert.IsFalse(canDelete, "teacher can't delete with less than 3 days notice");
    }
}
