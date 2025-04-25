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
            new() { FirstName = "Alice", LastName = "Johnson", Email = "alice.johnson@example.com", Password = "password1", Role = Role.Teacher },
            new() { FirstName = "Bob", LastName = "Martinez", Email = "bob.martinez@example.com", Password = "password2", Role = Role.Student },
            new() { FirstName = "Charlie", LastName = "Nguyen", Email = "charlie.nguyen@example.com", Password = "password3", Role = Role.Student },
            new() { FirstName = "Diana", LastName = "Reed", Email = "diana.reed@example.com", Password = "password4", Role = Role.Teacher },
            new() { FirstName = "Edward", LastName = "Kim", Email = "edward.kim@example.com", Password = "password5", Role = Role.Teacher },
            new() { FirstName = "Fiona", LastName = "Bennett", Email = "fiona.bennett@example.com", Password = "password6", Role = Role.Student }
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
            new() { RoomName = "Meeting Room 1", RoomType = RoomType.MeetingRoom },
            new() { RoomName = "Classroom 101", RoomType = RoomType.Classroom }
        };
        context.Room.AddRange(rooms);
        context.SaveChanges();

        // Bookings (valid bookings based on business logic)
        var bookings = new List<Booking>();
        var random = new Random();

        var nextMonday = DateTime.Today.AddDays(((int)DayOfWeek.Monday - (int)DateTime.Today.DayOfWeek + 7) % 7);
        var timeSlots = new[] { 8, 10, 12, 14 };

        var createdGroups = context.Group.ToList();
        var allUsers = context.User.ToList();
        var allRooms = context.Room.ToList();

        // Track bookings per slot per room
        var slotTracker = new Dictionary<(int roomId, DateTime start), List<Booking>>();

        foreach (var dayOffset in Enumerable.Range(0, 5)) // Mon–Fri
        {
            var date = nextMonday.AddDays(dayOffset);

            foreach (var hour in timeSlots)
            {
                var start = new DateTime(date.Year, date.Month, date.Day, hour, 0, 0);
                var end = start.AddHours(2);

                foreach (var room in allRooms)
                {
                    var key = (room.RoomId, start);
                    slotTracker.TryAdd(key, new List<Booking>());

                    var allowedBookings = room.RoomType == RoomType.Classroom ? 2 : 1;
                    if (slotTracker[key].Count >= allowedBookings)
                        continue;

                    // Pick a group that doesn't already have a future booking
                    var group = createdGroups.FirstOrDefault(g =>
                        !bookings.Any(b => b.GroupId == g.GroupId && b.EndTime > DateTime.Now));
                    if (group == null) continue;

                    // Pick a user that doesn't already have a conflicting booking
                    var user = allUsers.FirstOrDefault(u =>
                        !bookings.Any(b =>
                            b.CreatedByUserId == u.UserId &&
                            b.StartTime < end &&
                            b.EndTime > start));
                    if (user == null) continue;

                    // Determine smartboard usage
                    bool usesSmartboard = false;
                    if (room.RoomType == RoomType.MeetingRoom)
                    {
                        usesSmartboard = true;
                    }
                    else if (room.RoomType == RoomType.Classroom)
                    {
                        bool smartboardTaken = bookings.Any(b =>
                            b.RoomId == room.RoomId &&
                            b.StartTime == start &&
                            b.EndTime == end &&
                            b.UsesSmartboard);
                        usesSmartboard = !smartboardTaken && random.Next(0, 2) == 1;
                    }

                    var booking = new Booking
                    {
                        GroupId = group.GroupId,
                        RoomId = room.RoomId,
                        CreatedByUserId = user.UserId,
                        StartTime = start,
                        EndTime = end,
                        UsesSmartboard = usesSmartboard
                    };

                    bookings.Add(booking);
                    slotTracker[key].Add(booking);
                }
            }
        }

        context.Booking.AddRange(bookings);
        context.SaveChanges();
    }
}
