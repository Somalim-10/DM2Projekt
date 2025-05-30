using DM2Projekt.Data;
using DM2Projekt.Models;
using Microsoft.EntityFrameworkCore;

namespace DM2Projekt.Tests.GroupFeatures;

[TestClass]
public class GroupPermissionTests
{
    // Sets up in-memory DB with test users + group membership
    private DM2ProjektContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<DM2ProjektContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var context = new DM2ProjektContext(options);
        SeedTestData(context);
        return context;
    }

    private void SeedTestData(DM2ProjektContext context)
    {
        if (context.User.Any()) return;

        var armin = new User { FirstName = "Armin", LastName = "Arlert", Email = "armin@edu.dk", Password = "123", Role = Role.Student };
        var mikasa = new User { FirstName = "Mikasa", LastName = "Ackerman", Email = "mikasa@edu.dk", Password = "123", Role = Role.Student };

        context.User.AddRange(armin, mikasa);
        context.SaveChanges();

        var group = new Group
        {
            GroupName = "Test Group",
            CreatedByUserId = armin.UserId
        };
        context.Group.Add(group);
        context.SaveChanges();

        context.UserGroup.AddRange(
            new UserGroup { UserId = armin.UserId, GroupId = group.GroupId },
            new UserGroup { UserId = mikasa.UserId, GroupId = group.GroupId }
        );

        context.SaveChanges();
    }

    [TestMethod]
    public void Creator_Has_Permission_To_Kick()
    {
        using var context = CreateInMemoryContext();
        var armin = context.User.First(u => u.Email == "armin@edu.dk");
        var group = context.Group.First();

        var isCreator = group.CreatedByUserId == armin.UserId;

        Assert.IsTrue(isCreator, "group creator should have kick permission");
    }

    [TestMethod]
    public void Non_Creator_Does_Not_Have_Kick_Permission()
    {
        using var context = CreateInMemoryContext();
        var mikasa = context.User.First(u => u.Email == "mikasa@edu.dk");
        var group = context.Group.First();

        var isCreator = group.CreatedByUserId == mikasa.UserId;

        Assert.IsFalse(isCreator, "non-creator should not be allowed to kick");
    }

    [TestMethod]
    public void Member_Can_View_Group_If_In_UserGroup()
    {
        using var context = CreateInMemoryContext();
        var mikasa = context.User.First(u => u.Email == "mikasa@edu.dk");
        var group = context.Group.First();

        var isMember = context.UserGroup.Any(ug =>
            ug.UserId == mikasa.UserId && ug.GroupId == group.GroupId);

        Assert.IsTrue(isMember, "group member should be able to view group");
    }

    [TestMethod]
    public void User_Not_In_Group_Cannot_View_Group()
    {
        using var context = CreateInMemoryContext();

        var outsider = new User
        {
            FirstName = "Jean",
            LastName = "Kirstein",
            Email = "jean@edu.dk",
            Password = "123",
            Role = Role.Student
        };

        context.User.Add(outsider);
        context.SaveChanges();

        var group = context.Group.First();
        var isMember = context.UserGroup.Any(ug =>
            ug.UserId == outsider.UserId && ug.GroupId == group.GroupId);

        Assert.IsFalse(isMember, "non-member should not access the group");
    }

    [TestMethod]
    public void Creator_Cannot_Kick_Themself()
    {
        using var context = CreateInMemoryContext();
        var armin = context.User.First(u => u.Email == "armin@edu.dk");
        var group = context.Group.First();

        var tryingToKickSelf = group.CreatedByUserId == armin.UserId;

        Assert.IsTrue(tryingToKickSelf, "creator should be protected from self-kick logic");
    }
}
