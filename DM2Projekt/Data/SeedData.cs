using DM2Projekt.Models;
using DM2Projekt.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace DM2Projekt.Data;

// this class adds fake/test data to database
public static class SeedData
{
    public static void Initialize(IServiceProvider serviceProvider)
    {
        // make database context
        using var context = new DM2ProjektContext(
            serviceProvider.GetRequiredService<DbContextOptions<DM2ProjektContext>>());

        // if any data already there, stop
        if (context.User.Any() || context.Room.Any() || context.Group.Any())
            return;

        // make users
        var users = new List<User>
        {
            new() { FirstName = "Alice", LastName = "Johnson", Email = "alice.johnson@zealand.dk", Password = "password1", Role = Role.Teacher },
            new() { FirstName = "Bob", LastName = "Martinez", Email = "bob.martinez@edu.zealand.dk", Password = "password2", Role = Role.Student },
            new() { FirstName = "Charlie", LastName = "Nguyen", Email = "charlie.nguyen@edu.zealand.dk", Password = "password3", Role = Role.Student },
            new() { FirstName = "Diana", LastName = "Reed", Email = "diana.reed@zealand.dk", Password = "password4", Role = Role.Teacher },
            new() { FirstName = "Edward", LastName = "Kim", Email = "edward.kim@zealand.dk", Password = "password5", Role = Role.Teacher },
            new() { FirstName = "Fiona", LastName = "Bennett", Email = "fiona.bennett@edu.zealand.dk", Password = "password6", Role = Role.Student },
            new() { FirstName = "Samuel", LastName = "Andersen", Email = "samuel.andersen@zealand.dk", Password = "admin123", Role = Role.Admin }
        };
        context.User.AddRange(users);

        // make groups
        var groups = new List<Group>
        {
            new() { GroupName = "Group Alpha" },
            new() { GroupName = "Group Beta" },
            new() { GroupName = "Group Gamma" }
        };
        context.Group.AddRange(groups);

        // make rooms
        var rooms = new List<Room>
        {
            new() { RoomName = "Meeting Room 1", RoomType = RoomType.MeetingRoom },
            new() { RoomName = "Classroom 101", RoomType = RoomType.Classroom }
        };
        context.Room.AddRange(rooms);

        // now save users, groups and rooms
        context.SaveChanges();

        // link users to groups
        var userGroups = new List<UserGroup>
        {
            new() { UserId = 2, GroupId = 1 },
            new() { UserId = 3, GroupId = 1 },
            new() { UserId = 2, GroupId = 2 },
            new() { UserId = 6, GroupId = 3 },
            new() { UserId = 3, GroupId = 3 }
        };
        context.UserGroup.AddRange(userGroups);

        // make bookings
        var bookings = new List<Booking>();
        var random = new Random();

        // find next Monday
        var nextMonday = DateTime.Today.AddDays(((int)DayOfWeek.Monday - (int)DateTime.Today.DayOfWeek + 7) % 7);
        var timeSlots = new[] { 8, 10, 12, 14 };

        var createdGroups = context.Group.ToList();
        var allUsers = context.User
            .Where(u => u.Role == Role.Student || u.Role == Role.Admin) // only students/admins can create
            .ToList();
        var allRooms = context.Room.ToList();

        var slotTracker = new Dictionary<(int roomId, DateTime start), List<Booking>>();

        foreach (var dayOffset in Enumerable.Range(0, 5))
        {
            var date = nextMonday.AddDays(dayOffset);

            foreach (var hour in timeSlots)
            {
                var start = new DateTime(date.Year, date.Month, date.Day, hour, 0, 0);
                var end = start.AddHours(2);

                foreach (var room in allRooms)
                {
                    var key = (room.RoomId, start);
                    slotTracker.TryAdd(key, []);

                    var allowedBookings = room.RoomType == RoomType.Classroom ? 2 : 1;
                    if (slotTracker[key].Count >= allowedBookings)
                        continue;

                    var group = createdGroups.FirstOrDefault(g =>
                        !bookings.Any(b => b.GroupId == g.GroupId && b.EndTime > DateTime.Now));
                    if (group == null) continue;

                    var user = allUsers.FirstOrDefault(u =>
                        !bookings.Any(b =>
                            b.CreatedByUserId == u.UserId &&
                            b.StartTime < end &&
                            b.EndTime > start));
                    if (user == null) continue;

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
