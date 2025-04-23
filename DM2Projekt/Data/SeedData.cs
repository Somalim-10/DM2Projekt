using DM2Projekt.Models;
using DM2Projekt.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace DM2Projekt.Data;

public static class SeedData
{
    public static void Initialize(IServiceProvider serviceProvider)
    {
        using var context = new DM2ProjektContext(
            serviceProvider.GetRequiredService<DbContextOptions<DM2ProjektContext>>());

        // Avoid reseeding if data exists
        if (context.User.Any() || context.Room.Any() || context.Group.Any())
            return;

        // Users
        var users = new List<User>
        {
            new() { FirstName = "Alice", LastName = "Admin", Email = "alice@example.com", Role = Role.Teacher },
            new() { FirstName = "Bob", LastName = "Student", Email = "bob@example.com", Role = Role.Student },
            new() { FirstName = "Charlie", LastName = "Student", Email = "charlie@example.com", Role = Role.Student }
        };
        context.User.AddRange(users);

        // Groups
        var groups = new List<Group>
        {
            new() { GroupName = "Group Alpha" },
            new() { GroupName = "Group Beta" }
        };
        context.Group.AddRange(groups);
        context.SaveChanges();

        // UserGroups
        var userGroups = new List<UserGroup>
        {
            new() { UserId = 2, GroupId = 1 },
            new() { UserId = 3, GroupId = 1 },
            new() { UserId = 2, GroupId = 2 }
        };
        context.UserGroup.AddRange(userGroups);

        // Rooms
        var rooms = new List<Room>
        {
            new() { RoomName = "Auditorium A", Capacity = 100, RoomType = RoomType.Auditorium, CanBeShared = false },
            new() { RoomName = "Lab 1", Capacity = 20, RoomType = RoomType.Laboratory, CanBeShared = true }
        };
        context.Room.AddRange(rooms);
        context.SaveChanges();

        // Smartboards (1 per room)
        var smartboards = new List<Smartboard>
        {
            new() { RoomId = 1, IsAvailable = true },
            new() { RoomId = 2, IsAvailable = false }
        };
        context.Smartboard.AddRange(smartboards);
        context.SaveChanges();

        // Bookings
        var bookings = new List<Booking>
        {
            new()
            {
                GroupId = 1,
                RoomId = 1,
                CreatedByUserId = 1,
                SmartboardId = 1,
                StartTime = DateTime.Now.AddDays(1).AddHours(9),
                EndTime = DateTime.Now.AddDays(1).AddHours(11),
                Status = BookingStatus.Confirmed
            },
            new()
            {
                GroupId = 2,
                RoomId = 2,
                CreatedByUserId = 1,
                SmartboardId = null,
                StartTime = DateTime.Now.AddDays(2).AddHours(13),
                EndTime = DateTime.Now.AddDays(2).AddHours(15),
                Status = BookingStatus.Pending
            }
        };
        context.Booking.AddRange(bookings);

        context.SaveChanges();
    }
}
