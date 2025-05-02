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

        // skip seeding if data exists
        if (context.User.Any() || context.Room.Any() || context.Group.Any())
            return;

        // ✅ USERS
        var users = new List<User>
        {
            new() { FirstName = "Admin", LastName = "Zealand", Email = "admin@zealand.dk", Password = "admin", Role = Role.Admin },
            new() { FirstName = "Erwin", LastName = "Smith", Email = "erwin@zealand.dk", Password = "123", Role = Role.Teacher },
            new() { FirstName = "Armin", LastName = "Arlert", Email = "armin@edu.zealand.dk", Password = "123", Role = Role.Student },
            new() { FirstName = "Mikasa", LastName = "Ackerman", Email = "mikasa@edu.zealand.dk", Password = "123", Role = Role.Student },
            new() { FirstName = "Levi", LastName = "Ackerman", Email = "levi@edu.zealand.dk", Password = "123", Role = Role.Student },
            new() { FirstName = "Hange", LastName = "Zoe", Email = "hange@zealand.dk", Password = "123", Role = Role.Teacher },
            new() { FirstName = "Jean", LastName = "Kirstein", Email = "jean@edu.zealand.dk", Password = "123", Role = Role.Student }
        };
        context.User.AddRange(users);
        context.SaveChanges();

        // fetch some for relationships
        var armin = context.User.First(u => u.Email == "armin@edu.zealand.dk");
        var mikasa = context.User.First(u => u.Email == "mikasa@edu.zealand.dk");
        var levi = context.User.First(u => u.Email == "levi@edu.zealand.dk");
        var jean = context.User.First(u => u.Email == "jean@edu.zealand.dk");

        // ✅ GROUPS
        var group1 = new Group { GroupName = "Scouting Legion", CreatedByUserId = armin.UserId };
        var group2 = new Group { GroupName = "Wall Defenders", CreatedByUserId = mikasa.UserId };

        context.Group.AddRange(group1, group2);
        context.SaveChanges();

        // ✅ GROUP MEMBERS
        var memberships = new List<UserGroup>
        {
            new() { GroupId = group1.GroupId, UserId = armin.UserId },
            new() { GroupId = group1.GroupId, UserId = mikasa.UserId },
            new() { GroupId = group2.GroupId, UserId = mikasa.UserId },
            new() { GroupId = group2.GroupId, UserId = levi.UserId }
        };
        context.UserGroup.AddRange(memberships);
        context.SaveChanges();

        // ✅ PENDING INVITES
        var invite1 = new GroupInvitation
        {
            GroupId = group1.GroupId,
            InvitedUserId = jean.UserId,
            SentAt = DateTime.Now,
            IsAccepted = null
        };
        context.GroupInvitation.Add(invite1);
        context.SaveChanges();
 
        // ✅ ROOMS
        var rooms = new List<Room>
        {
            new() { RoomName = "HQ Conference Room", RoomType = RoomType.MeetingRoom, ImageUrl="https://www.hoteljosef.com/wp-content/uploads/2024/06/Prague-small-conference-room.jpg" },
            new() { RoomName = "Training Hall", RoomType = RoomType.Classroom, ImageUrl="https://www.appliedglobal.com/wp-content/uploads/How-to-Create-a-Modern-Meeting-Room-Setup.png" }
        };
        context.Room.AddRange(rooms);
        context.SaveChanges();

        // ✅ BOOKINGS
        var random = new Random();
        var monday = new DateTime(2025, 5, 5); // next Monday, hardcoded to avoid dynamic inconsistencies
        var timeSlots = new[] { 8, 10, 12, 14 };
        var bookings = new List<Booking>();

        foreach (var hour in timeSlots)
        {
            var start = new DateTime(monday.Year, monday.Month, monday.Day, hour, 0, 0);
            var end = start.AddHours(2);

            bookings.Add(new Booking
            {
                GroupId = group1.GroupId,
                RoomId = rooms[0].RoomId,
                CreatedByUserId = armin.UserId,
                StartTime = start,
                EndTime = end,
                UsesSmartboard = true
            });

            bookings.Add(new Booking
            {
                GroupId = group2.GroupId,
                RoomId = rooms[1].RoomId,
                CreatedByUserId = mikasa.UserId,
                StartTime = start,
                EndTime = end,
                UsesSmartboard = random.Next(0, 2) == 1
            });
        }

        context.Booking.AddRange(bookings);
        context.SaveChanges();
    }
}
