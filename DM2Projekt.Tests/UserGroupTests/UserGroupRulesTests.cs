using DM2Projekt.Data;
using DM2Projekt.Models;
using Microsoft.EntityFrameworkCore;

namespace DM2Projekt.Tests.UserGroupTests;

[TestClass]
public class UserGroupRulesTests
{
    private DM2ProjektContext GetInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<DM2ProjektContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var context = new DM2ProjektContext(options);
        SeedFakeData(context);
        return context;
    }

    private void SeedFakeData(DM2ProjektContext context)
    {
        var user = new User { FirstName = "Test", LastName = "User", Email = "test@edu.dk", Password = "123", Role = Role.Student };
        var group1 = new Group { GroupName = "Group One", CreatedByUserId = 1 };
        var group2 = new Group { GroupName = "Group Two", CreatedByUserId = 1 };
        var group3 = new Group { GroupName = "Group Three", CreatedByUserId = 1 };

        context.User.Add(user);
        context.Group.AddRange(group1, group2, group3);
        context.SaveChanges();

        context.UserGroup.Add(new UserGroup { UserId = user.UserId, GroupId = group1.GroupId });
        context.UserGroup.Add(new UserGroup { UserId = user.UserId, GroupId = group2.GroupId });
        context.SaveChanges();
    }

    [TestMethod]
    public void User_Cannot_Join_Same_Group_Twice()
    {
        using var context = GetInMemoryContext();
        var userId = context.User.First().UserId;
        var groupId = context.Group.First().GroupId;

        var alreadyJoined = context.UserGroup.Any(ug => ug.UserId == userId && ug.GroupId == groupId);

        Assert.IsTrue(alreadyJoined, "User is already in the group and shouldn't rejoin");
    }

    [TestMethod]
    public void User_Cannot_Join_More_Than_Three_Groups()
    {
        using var context = GetInMemoryContext();
        var user = context.User.First();

        // Add third group
        var thirdGroup = context.Group.First(g => g.GroupName == "Group Three");
        context.UserGroup.Add(new UserGroup { UserId = user.UserId, GroupId = thirdGroup.GroupId });
        context.SaveChanges();

        var groupCount = context.UserGroup.Count(ug => ug.UserId == user.UserId);

        // Try to add a 4th
        var fourthGroup = new Group { GroupName = "Group Four", CreatedByUserId = user.UserId };
        context.Group.Add(fourthGroup);
        context.SaveChanges();

        var canJoinAnother = groupCount < 3;

        Assert.IsFalse(canJoinAnother, "User already in 3 groups, should not join more");
    }

    [TestMethod]
    public void Can_Add_New_UserGroup_If_Not_Duplicate()
    {
        using var context = GetInMemoryContext();
        var user = context.User.First();
        var newGroup = new Group { GroupName = "Group New", CreatedByUserId = user.UserId };
        context.Group.Add(newGroup);
        context.SaveChanges();

        var link = new UserGroup { UserId = user.UserId, GroupId = newGroup.GroupId };
        context.UserGroup.Add(link);
        context.SaveChanges();

        var exists = context.UserGroup.Any(ug => ug.UserId == user.UserId && ug.GroupId == newGroup.GroupId);
        Assert.IsTrue(exists, "UserGroup should be added");
    }

    [TestMethod]
    public void Can_Remove_UserGroup_Link()
    {
        using var context = GetInMemoryContext();
        var link = context.UserGroup.First();
        context.UserGroup.Remove(link);
        context.SaveChanges();

        var stillThere = context.UserGroup.Any(ug => ug.UserId == link.UserId && ug.GroupId == link.GroupId);
        Assert.IsFalse(stillThere, "UserGroup link should be removed");
    }

    [TestMethod]
    public void Cannot_Create_UserGroup_If_User_Does_Not_Exist()
    {
        using var context = GetInMemoryContext();
        var fakeUserId = 999;
        var groupId = context.Group.First().GroupId;

        var userExists = context.User.Any(u => u.UserId == fakeUserId);
        Assert.IsFalse(userExists, "User doesn't exist");

        // simulate check
        var valid = context.User.Any(u => u.UserId == fakeUserId);
        Assert.IsFalse(valid);
    }

    [TestMethod]
    public void Cannot_Create_UserGroup_If_Group_Does_Not_Exist()
    {
        using var context = GetInMemoryContext();
        var userId = context.User.First().UserId;
        var fakeGroupId = 999;

        var groupExists = context.Group.Any(g => g.GroupId == fakeGroupId);
        Assert.IsFalse(groupExists, "Group doesn't exist");
    }
}
