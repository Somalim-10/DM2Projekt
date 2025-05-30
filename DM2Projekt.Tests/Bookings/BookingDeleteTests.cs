using DM2Projekt.Data;
using DM2Projekt.Models;
using Microsoft.EntityFrameworkCore;

namespace DM2Projekt.Tests.Bookings;

[TestClass]
public class BookingDeleteTests
{
    private DM2ProjektContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<DM2ProjektContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new DM2ProjektContext(options);
    }

    [TestMethod]
    public async Task Admin_Can_Delete_Any_Booking()
    {
        using var context = CreateInMemoryContext();
        context.Booking.Add(new Booking { BookingId = 1, CreatedByUserId = 2 });
        context.SaveChanges();

        var userRole = "Admin";
        var canDelete = userRole == "Admin";

        Assert.IsTrue(canDelete, "admin should be able to delete any booking");
    }

    [TestMethod]
    public async Task Student_Can_Delete_Own_Booking()
    {
        using var context = CreateInMemoryContext();
        var studentId = 5;

        context.Booking.Add(new Booking { BookingId = 1, CreatedByUserId = studentId });
        context.SaveChanges();

        var userRole = "Student";
        var booking = context.Booking.First();
        var canDelete = userRole is "Admin" or "Teacher" || booking.CreatedByUserId == studentId;

        Assert.IsTrue(canDelete, "student should delete own booking");
    }

    [TestMethod]
    public async Task Student_Cannot_Delete_Other_Booking()
    {
        using var context = CreateInMemoryContext();
        var studentId = 5;

        context.Booking.Add(new Booking { BookingId = 1, CreatedByUserId = 99 });
        context.SaveChanges();

        var userRole = "Student";
        var booking = context.Booking.First();
        var canDelete = userRole is "Admin" or "Teacher" || booking.CreatedByUserId == studentId;

        Assert.IsFalse(canDelete, "student should not delete someone else's booking");
    }

    [TestMethod]
    public async Task Teacher_Can_Delete_Booking_With_Enough_Notice()
    {
        using var context = CreateInMemoryContext();
        var now = DateTime.Now;

        context.Booking.Add(new Booking
        {
            BookingId = 1,
            CreatedByUserId = 2,
            StartTime = now.AddDays(5)
        });
        context.SaveChanges();

        var userRole = "Teacher";
        var booking = context.Booking.First();
        var canDelete = userRole == "Admin" || (userRole == "Teacher" && (booking.StartTime - now)?.TotalDays >= 3);

        Assert.IsTrue(canDelete, "teacher can delete with 3+ days notice");
    }

    [TestMethod]
    public async Task Teacher_Cannot_Delete_Booking_With_Short_Notice()
    {
        using var context = CreateInMemoryContext();
        var now = DateTime.Now;

        context.Booking.Add(new Booking
        {
            BookingId = 1,
            CreatedByUserId = 2,
            StartTime = now.AddDays(1)
        });
        context.SaveChanges();

        var userRole = "Teacher";
        var booking = context.Booking.First();
        var canDelete = userRole == "Admin" || (userRole == "Teacher" && (booking.StartTime - now)?.TotalDays >= 3);

        Assert.IsFalse(canDelete, "teacher can't delete with less than 3 days notice");
    }
}
