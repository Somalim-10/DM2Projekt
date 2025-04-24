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

        if (context.User.Any() || context.Room.Any() || context.Group.Any())
            return;

        // Users
        var users = new List<User>
        {
            new() { FirstName = "Alice", LastName = "Johnson", Email = "alice.johnson@example.com", Role = Role.Teacher },
            new() { FirstName = "Bob", LastName = "Martinez", Email = "bob.martinez@example.com", Role = Role.Student },
            new() { FirstName = "Charlie", LastName = "Nguyen", Email = "charlie.nguyen@example.com", Role = Role.Student },
            new() { FirstName = "Diana", LastName = "Reed", Email = "diana.reed@example.com", Role = Role.Teacher },
            new() { FirstName = "Edward", LastName = "Kim", Email = "edward.kim@example.com", Role = Role.Teacher },
            new() { FirstName = "Fiona", LastName = "Bennett", Email = "fiona.bennett@example.com", Role = Role.Student }
        };
        context.User.AddRange(users);

        // Groups
        var groups = new List<Group>
        {
            new() { GroupName = "Group Alpha" },
            new() { GroupName = "Group Beta" },
            new() { GroupName = "Group Gamma" }
        };
        context.Group.AddRange(groups);
        context.SaveChanges();

        // UserGroups
        var userGroups = new List<UserGroup>
        {
            new() { UserId = 2, GroupId = 1 },
            new() { UserId = 3, GroupId = 1 },
            new() { UserId = 2, GroupId = 2 },
            new() { UserId = 6, GroupId = 3 },
            new() { UserId = 3, GroupId = 3 }
        };
        context.UserGroup.AddRange(userGroups);

        // Rooms
        var rooms = new List<Room>
        {
            new() { RoomName = "Meeting Room 1",   RoomType = RoomType.MeetingRoom},
            new() { RoomName = "Classroom 101", RoomType = RoomType.Classroom }
        };
        context.Room.AddRange(rooms);
        context.SaveChanges();

        // Smartboards
        var smartboards = new List<Smartboard>
        {
            new() { RoomId = 1, IsAvailable = true },
            new() { RoomId = 2, IsAvailable = false },
            new() { RoomId = 3, IsAvailable = true },
            new() { RoomId = 4, IsAvailable = true }
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
            },
            new()
            {
                GroupId = 2,
                RoomId = 2,
                CreatedByUserId = 1,
                SmartboardId = null,
                StartTime = DateTime.Now.AddDays(2).AddHours(13),
                EndTime = DateTime.Now.AddDays(2).AddHours(15),
            },
            new()
            {
                GroupId = 3,
                RoomId = 3,
                CreatedByUserId = 4,
                SmartboardId = 3,
                StartTime = DateTime.Now.AddDays(1).AddHours(10),
                EndTime = DateTime.Now.AddDays(1).AddHours(12),
            },
            new()
            {
                GroupId = 1,
                RoomId = 4,
                CreatedByUserId = 5,
                SmartboardId = 4,
                StartTime = DateTime.Now.AddDays(3).AddHours(8),
                EndTime = DateTime.Now.AddDays(3).AddHours(10),
            }
        };
        context.Booking.AddRange(bookings);

        context.SaveChanges();
    }
}
