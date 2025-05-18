using DM2Projekt.Models;
using Microsoft.EntityFrameworkCore;

namespace DM2Projekt.Data;

// fills DB with default data if empty
public static class SeedData
{
    public static void Initialize(IServiceProvider serviceProvider)
    {
        using var context = new DM2ProjektContext(
            serviceProvider.GetRequiredService<DbContextOptions<DM2ProjektContext>>());

        // DB already seeded
        if (context.User.Any() || context.Room.Any() || context.Group.Any())
            return;

        // avatar images (DiceBear)
        var avatars = new Dictionary<string, string>
        {
            ["Brian"] = "https://api.dicebear.com/9.x/adventurer/svg?seed=Brian",
            ["Vivian"] = "https://api.dicebear.com/9.x/adventurer/svg?seed=Vivian",
            ["Andrea"] = "https://api.dicebear.com/9.x/adventurer/svg?seed=Andrea",
            ["Katherine"] = "https://api.dicebear.com/9.x/adventurer/svg?seed=Katherine",
            ["Valentina"] = "https://api.dicebear.com/9.x/adventurer/svg?seed=Valentina",
            ["Easton"] = "https://api.dicebear.com/9.x/adventurer/svg?seed=Easton",
            ["Mason"] = "https://api.dicebear.com/9.x/adventurer/svg?seed=Mason",
            ["Mackenzie"] = "https://api.dicebear.com/9.x/adventurer/svg?seed=Mackenzie"
        };

        // create users
        var users = new List<User>
        {
            new() { FirstName = "Admin", LastName = "Zealand", Email = "admin@zealand.dk", Password = "admin", Role = Role.Admin },
            new() { FirstName = "Martin", LastName = "Jensen", Email = "mj@zealand.dk", Password = "teacher1", Role = Role.Teacher, ProfileImagePath = avatars["Brian"] },
            new() { FirstName = "Anne", LastName = "Larsen", Email = "al@zealand.dk", Password = "teacher2", Role = Role.Teacher, ProfileImagePath = avatars["Vivian"] },
            new() { FirstName = "Sara", LastName = "Hansen", Email = "sara.h@edu.zealand.dk", Password = "student1", Role = Role.Student, ProfileImagePath = avatars["Andrea"] },
            new() { FirstName = "Jonas", LastName = "Møller", Email = "jonas.m@edu.zealand.dk", Password = "student2", Role = Role.Student, ProfileImagePath = avatars["Easton"] },
            new() { FirstName = "Katrine", LastName = "Nielsen", Email = "katrine.n@edu.zealand.dk", Password = "student3", Role = Role.Student, ProfileImagePath = avatars["Katherine"] },
            new() { FirstName = "Ali", LastName = "Mahmoud", Email = "ali.m@edu.zealand.dk", Password = "student4", Role = Role.Student, ProfileImagePath = avatars["Mason"] },
            new() { FirstName = "Frederik", LastName = "Andersen", Email = "frederik.a@edu.zealand.dk", Password = "student5", Role = Role.Student },
            new() { FirstName = "Line", LastName = "Christensen", Email = "line.c@edu.zealand.dk", Password = "student6", Role = Role.Student, ProfileImagePath = avatars["Valentina"] },
            new() { FirstName = "Emil", LastName = "Petersen", Email = "emil.p@edu.zealand.dk", Password = "student7", Role = Role.Student, ProfileImagePath = avatars["Mackenzie"] },
            new() { FirstName = "Julie", LastName = "Olsen", Email = "julie.o@edu.zealand.dk", Password = "student8", Role = Role.Student },
            new() { FirstName = "Sebastian", LastName = "Friis", Email = "sebastian.f@edu.zealand.dk", Password = "student9", Role = Role.Student }
        };
        context.User.AddRange(users);
        context.SaveChanges();

        // groups – owned by users (by index)
        var group404 = new Group { GroupName = "404 Not Found", CreatedByUserId = users[3].UserId };
        var groupByteMe = new Group { GroupName = "Byte Me", CreatedByUserId = users[4].UserId };
        var groupNullSquad = new Group { GroupName = "Null Squad", CreatedByUserId = users[5].UserId };

        context.Group.AddRange(group404, groupByteMe, groupNullSquad);
        context.SaveChanges();

        // user <-> group links
        var memberships = new List<UserGroup>
        {
            new() { GroupId = group404.GroupId, UserId = users[3].UserId },
            new() { GroupId = group404.GroupId, UserId = users[4].UserId },
            new() { GroupId = group404.GroupId, UserId = users[6].UserId },

            new() { GroupId = groupByteMe.GroupId, UserId = users[4].UserId },
            new() { GroupId = groupByteMe.GroupId, UserId = users[5].UserId },
            new() { GroupId = groupByteMe.GroupId, UserId = users[7].UserId },

            new() { GroupId = groupNullSquad.GroupId, UserId = users[5].UserId },
            new() { GroupId = groupNullSquad.GroupId, UserId = users[8].UserId },
            new() { GroupId = groupNullSquad.GroupId, UserId = users[9].UserId }
        };
        context.UserGroup.AddRange(memberships);
        context.SaveChanges();

        // images for rooms
        var classroomImages = new[]
        {
            "https://classrooms.uiowa.edu/sites/classrooms.uiowa.edu/files/styles/large/public/2022-03/VAN%20362%20Classroom%202.jpg?itok=alGZu7qW",
            "https://classrooms.uiowa.edu/sites/classrooms.uiowa.edu/files/styles/large/public/2022-03/AJB%20W240_4%20052919.jpg?itok=M4jCJmKL",
            "https://classrooms.uiowa.edu/sites/classrooms.uiowa.edu/files/styles/large/public/2022-03/VAN%20470%20Classroom%202.jpg?itok=1HdW9gpU"
        };

        var meetingRoomImages = new[]
        {
            "https://www.kramerav.com/wp-content/uploads/2023/03/large-meeting-room-cam02-10-1-1440x960.jpg",
            "https://www.eposaudio.com/contentassets/2af3669017f34ae58049ce43c127bd3b/expand_idealmeetingroom_still-life_01.jpg",
            "https://www.servicedofficecompany.co.uk/wp-content/uploads/2022/01/meetingroom-2.jpg"
        };

        // seed rooms
        var rooms = new List<Room>
        {
            new() { RoomName = "Paris", RoomType = RoomType.MeetingRoom, Building = Building.A, Floor = Floor.First, ImageUrl = meetingRoomImages[0] },
            new() { RoomName = "Berlin", RoomType = RoomType.Classroom, Building = Building.B, Floor = Floor.Second, ImageUrl = classroomImages[0] },
            new() { RoomName = "Rome", RoomType = RoomType.MeetingRoom, Building = Building.C, Floor = Floor.Ground, ImageUrl = meetingRoomImages[1] },
            new() { RoomName = "Lisbon", RoomType = RoomType.Classroom, Building = Building.D, Floor = Floor.Third, ImageUrl = classroomImages[1] },
            new() { RoomName = "Vienna", RoomType = RoomType.MeetingRoom, Building = Building.A, Floor = Floor.Second, ImageUrl = meetingRoomImages[2] },
            new() { RoomName = "Oslo", RoomType = RoomType.Classroom, Building = Building.B, Floor = Floor.First, ImageUrl = classroomImages[2] }
        };

        context.Room.AddRange(rooms);
        context.SaveChanges();
    }
}
