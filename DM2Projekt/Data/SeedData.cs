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

        // skip seeding if there's already data
        if (context.User.Any() || context.Room.Any() || context.Group.Any())
            return;

        // --------------------------
        // AVATAR URLS (DiceBear)
        // --------------------------
        var avatarBrian = "https://api.dicebear.com/9.x/adventurer/svg?seed=Brian";
        var avatarVivian = "https://api.dicebear.com/9.x/adventurer/svg?seed=Vivian";
        var avatarAndrea = "https://api.dicebear.com/9.x/adventurer/svg?seed=Andrea";
        var avatarKatherine = "https://api.dicebear.com/9.x/adventurer/svg?seed=Katherine";
        var avatarValentina = "https://api.dicebear.com/9.x/adventurer/svg?seed=Valentina";
        var avatarEaston = "https://api.dicebear.com/9.x/adventurer/svg?seed=Easton";
        var avatarMason = "https://api.dicebear.com/9.x/adventurer/svg?seed=Mason";
        var avatarMackenzie = "https://api.dicebear.com/9.x/adventurer/svg?seed=Mackenzie";

        // --------------------------
        // USERS
        // --------------------------
        var users = new List<User>
        {
            // admin
            new() { FirstName = "Admin", LastName = "Zealand", Email = "admin@zealand.dk", Password = "admin", Role = Role.Admin },

            // teachers
            new() { FirstName = "Martin", LastName = "Jensen", Email = "mj@zealand.dk", Password = "teacher1", Role = Role.Teacher, ProfileImagePath = avatarBrian },
            new() { FirstName = "Anne", LastName = "Larsen", Email = "al@zealand.dk", Password = "teacher2", Role = Role.Teacher, ProfileImagePath = avatarVivian },

            // students
            new() { FirstName = "Sara", LastName = "Hansen", Email = "sara.h@edu.zealand.dk", Password = "student1", Role = Role.Student, ProfileImagePath = avatarAndrea },
            new() { FirstName = "Jonas", LastName = "Møller", Email = "jonas.m@edu.zealand.dk", Password = "student2", Role = Role.Student, ProfileImagePath = avatarEaston },
            new() { FirstName = "Katrine", LastName = "Nielsen", Email = "katrine.n@edu.zealand.dk", Password = "student3", Role = Role.Student, ProfileImagePath = avatarKatherine },
            new() { FirstName = "Ali", LastName = "Mahmoud", Email = "ali.m@edu.zealand.dk", Password = "student4", Role = Role.Student, ProfileImagePath = avatarMason },
            new() { FirstName = "Frederik", LastName = "Andersen", Email = "frederik.a@edu.zealand.dk", Password = "student5", Role = Role.Student },
            new() { FirstName = "Line", LastName = "Christensen", Email = "line.c@edu.zealand.dk", Password = "student6", Role = Role.Student, ProfileImagePath = avatarValentina },
            new() { FirstName = "Emil", LastName = "Petersen", Email = "emil.p@edu.zealand.dk", Password = "student7", Role = Role.Student, ProfileImagePath = avatarMackenzie },
            new() { FirstName = "Julie", LastName = "Olsen", Email = "julie.o@edu.zealand.dk", Password = "student8", Role = Role.Student },
            new() { FirstName = "Sebastian", LastName = "Friis", Email = "sebastian.f@edu.zealand.dk", Password = "student9", Role = Role.Student }
        };
        context.User.AddRange(users);
        context.SaveChanges();

        // --------------------------
        // GROUPS
        // --------------------------
        var groups = new List<Group>
        {
            new() { GroupName = "404 Not Found", CreatedByUserId = users[3].UserId },
            new() { GroupName = "Byte Me", CreatedByUserId = users[5].UserId },
            new() { GroupName = "Commit & Push", CreatedByUserId = users[4].UserId },
            new() { GroupName = "Agile Avengers", CreatedByUserId = users[6].UserId },
            new() { GroupName = "Coffee Overflow", CreatedByUserId = users[7].UserId },
            new() { GroupName = "Runtime Terrors", CreatedByUserId = users[9].UserId },
            new() { GroupName = "DevOps or Die", CreatedByUserId = users[10].UserId },
            new() { GroupName = "Final Final Group v2", CreatedByUserId = users[11].UserId }
        };
        context.Group.AddRange(groups);
        context.SaveChanges();

        // --------------------------
        // GROUP MEMBERSHIPS
        // --------------------------
        var memberships = new List<UserGroup>
        {
            new() { GroupId = groups[0].GroupId, UserId = users[3].UserId },
            new() { GroupId = groups[0].GroupId, UserId = users[4].UserId },

            new() { GroupId = groups[1].GroupId, UserId = users[5].UserId },
            new() { GroupId = groups[1].GroupId, UserId = users[10].UserId },

            new() { GroupId = groups[2].GroupId, UserId = users[4].UserId },
            new() { GroupId = groups[2].GroupId, UserId = users[11].UserId },

            new() { GroupId = groups[3].GroupId, UserId = users[6].UserId },
            new() { GroupId = groups[3].GroupId, UserId = users[9].UserId }
        };
        context.UserGroup.AddRange(memberships);
        context.SaveChanges();

        // --------------------------
        // ROOMS
        // --------------------------
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

        var rooms = new List<Room>
        {
            new() { RoomName = "Paris", RoomType = RoomType.MeetingRoom, Building = Building.A, Floor = Floor.First, BuildingName = "Building A", FloorName = "1st Floor", ImageUrl = meetingRoomImages[0] },
            new() { RoomName = "Berlin", RoomType = RoomType.Classroom, Building = Building.B, Floor = Floor.Second, BuildingName = "Building B", FloorName = "2nd Floor", ImageUrl = classroomImages[0] },
            new() { RoomName = "Rome", RoomType = RoomType.MeetingRoom, Building = Building.C, Floor = Floor.Ground, BuildingName = "Building C", FloorName = "Ground Floor", ImageUrl = meetingRoomImages[1] },
            new() { RoomName = "Lisbon", RoomType = RoomType.Classroom, Building = Building.D, Floor = Floor.Third, BuildingName = "Building D", FloorName = "3rd Floor", ImageUrl = classroomImages[1] },
            new() { RoomName = "Vienna", RoomType = RoomType.MeetingRoom, Building = Building.A, Floor = Floor.Second, BuildingName = "Building A", FloorName = "2nd Floor", ImageUrl = meetingRoomImages[2] },
            new() { RoomName = "Oslo", RoomType = RoomType.Classroom, Building = Building.B, Floor = Floor.First, BuildingName = "Building B", FloorName = "1st Floor", ImageUrl = classroomImages[2] }
        };
        context.Room.AddRange(rooms);
        context.SaveChanges();
    }
}
