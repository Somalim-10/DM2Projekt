using DM2Projekt.Data;
using DM2Projekt.Models;
using Microsoft.EntityFrameworkCore;

namespace DM2Projekt.Tests.GroupFeatures;

[TestClass]
public class GroupLifecycleTests
{
    // Builds a fresh database and loads test users, group, and invite
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
        var levi = new User { FirstName = "Levi", LastName = "Ackerman", Email = "levi@edu.dk", Password = "123", Role = Role.Student };

        context.User.AddRange(armin, mikasa, levi);
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

        context.GroupInvitation.Add(new GroupInvitation
        {
            GroupId = group.GroupId,
            InvitedUserId = levi.UserId,
            SentAt = DateTime.Now,
            IsAccepted = null
        });

        context.SaveChanges();
    }

    [TestMethod]
    public void Student_Cannot_Create_Multiple_Groups()
    {
        using var context = CreateInMemoryContext();
        var armin = context.User.First(u => u.Email == "armin@edu.dk");

        var alreadyCreated = context.Group.Any(g => g.CreatedByUserId == armin.UserId);
        var canCreateAnother = !alreadyCreated;

        Assert.IsTrue(alreadyCreated);
        Assert.IsFalse(canCreateAnother, "student should not create more than one group");
    }

    [TestMethod]
    public void Student_Cannot_Be_In_More_Than_Three_Groups()
    {
        using var context = CreateInMemoryContext();
        var mikasa = context.User.First(u => u.Email == "mikasa@edu.dk");

        // Add 2 more group memberships to reach 3
        for (int i = 0; i < 2; i++)
        {
            var extraGroup = new Group { GroupName = $"ExtraGroup{i + 1}", CreatedByUserId = mikasa.UserId };
            context.Group.Add(extraGroup);
            context.SaveChanges();

            context.UserGroup.Add(new UserGroup { UserId = mikasa.UserId, GroupId = extraGroup.GroupId });
        }

        context.SaveChanges();

        var groupCount = context.UserGroup.Count(ug => ug.UserId == mikasa.UserId);
        var canJoinAnother = groupCount < 3;

        Assert.AreEqual(3, groupCount);
        Assert.IsFalse(canJoinAnother, "should block joining more than 3 groups");
    }

    [TestMethod]
    public void Can_Invite_Student_Who_Is_Not_Already_Invited_Or_Member()
    {
        using var context = CreateInMemoryContext();
        var group = context.Group.First();
        var levi = context.User.First(u => u.Email == "levi@edu.dk");

        var alreadyInvited = context.GroupInvitation.Any(i =>
            i.GroupId == group.GroupId &&
            i.InvitedUserId == levi.UserId &&
            i.IsAccepted == null);

        var alreadyMember = context.UserGroup.Any(ug =>
            ug.GroupId == group.GroupId && ug.UserId == levi.UserId);

        var canInvite = !alreadyInvited && !alreadyMember;

        Assert.IsFalse(canInvite, "levi already invited, should not be invited again");
    }

    [TestMethod]
    public void Member_Can_Leave_Group()
    {
        using var context = CreateInMemoryContext();
        var mikasa = context.User.First(u => u.Email == "mikasa@edu.dk");
        var groupId = context.Group.First().GroupId;

        var membership = context.UserGroup.FirstOrDefault(ug =>
            ug.UserId == mikasa.UserId && ug.GroupId == groupId);

        context.UserGroup.Remove(membership);
        context.SaveChanges();

        var stillMember = context.UserGroup.Any(ug =>
            ug.UserId == mikasa.UserId && ug.GroupId == groupId);

        Assert.IsFalse(stillMember, "membership should be removed");
    }

    [TestMethod]
    public void Creator_Can_Kick_Member()
    {
        using var context = CreateInMemoryContext();
        var armin = context.User.First(u => u.Email == "armin@edu.dk");
        var mikasa = context.User.First(u => u.Email == "mikasa@edu.dk");
        var group = context.Group.First(g => g.CreatedByUserId == armin.UserId);

        var membership = context.UserGroup.FirstOrDefault(ug =>
            ug.UserId == mikasa.UserId && ug.GroupId == group.GroupId);

        context.UserGroup.Remove(membership);
        context.SaveChanges();

        var stillMember = context.UserGroup.Any(ug =>
            ug.UserId == mikasa.UserId && ug.GroupId == group.GroupId);

        Assert.IsFalse(stillMember, "member should be kicked");
    }
}
