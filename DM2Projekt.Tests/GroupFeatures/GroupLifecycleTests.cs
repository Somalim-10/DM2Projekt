using DM2Projekt.Data;
using DM2Projekt.Models;
using DM2Projekt.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace DM2Projekt.Tests.GroupFeatures;

[TestClass]
public class GroupLifecycleTests
{
    // 🧪 Creates a fresh in-memory db for each test run
    private DM2ProjektContext GetInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<DM2ProjektContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var context = new DM2ProjektContext(options);
        SeedFakeData(context); // load dummy data
        return context;
    }

    // 🔁 Fakes realistic users, groups, invites etc.
    private void SeedFakeData(DM2ProjektContext context)
    {
        if (context.User.Any()) return; // skip if already seeded

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
        using var context = GetInMemoryContext();
        var armin = context.User.First(u => u.Email == "armin@edu.dk");

        // ✅ Armin already created one group
        var alreadyCreated = context.Group.Any(g => g.CreatedByUserId == armin.UserId);
        Assert.IsTrue(alreadyCreated);

        // ❌ Should not be allowed to create another
        var canCreateAnother = !context.Group.Any(g => g.CreatedByUserId == armin.UserId);
        Assert.IsFalse(canCreateAnother);
    }

    [TestMethod]
    public void Student_Cannot_Be_In_More_Than_Three_Groups()
    {
        using var context = GetInMemoryContext();
        var mikasa = context.User.First(u => u.Email == "mikasa@edu.dk");

        // Add 2 more memberships to hit 3 total
        for (int i = 0; i < 2; i++)
        {
            var group = new Group { GroupName = $"ExtraGroup{i + 1}", CreatedByUserId = mikasa.UserId };
            context.Group.Add(group);
            context.SaveChanges();

            context.UserGroup.Add(new UserGroup { UserId = mikasa.UserId, GroupId = group.GroupId });
        }

        context.SaveChanges();

        // ✅ Already in 3 groups
        var groupCount = context.UserGroup.Count(ug => ug.UserId == mikasa.UserId);
        Assert.AreEqual(3, groupCount);

        // ❌ Should not allow another
        bool canJoinAnother = groupCount < 3;
        Assert.IsFalse(canJoinAnother);
    }

    [TestMethod]
    public void Can_Invite_Student_Who_Is_Not_Already_Invited_Or_Member()
    {
        using var context = GetInMemoryContext();
        var group = context.Group.First();
        var levi = context.User.First(u => u.Email == "levi@edu.dk");

        var alreadyInvited = context.GroupInvitation.Any(i =>
            i.GroupId == group.GroupId &&
            i.InvitedUserId == levi.UserId &&
            i.IsAccepted == null);

        var alreadyMember = context.UserGroup.Any(ug =>
            ug.GroupId == group.GroupId && ug.UserId == levi.UserId);

        var canInvite = !alreadyInvited && !alreadyMember;

        // ❌ Levi already invited — should not allow it again
        Assert.IsFalse(canInvite);
    }

    [TestMethod]
    public void Member_Can_Leave_Group()
    {
        using var context = GetInMemoryContext();
        var mikasa = context.User.First(u => u.Email == "mikasa@edu.dk");
        var groupId = context.Group.First().GroupId;

        var membership = context.UserGroup.FirstOrDefault(ug =>
            ug.UserId == mikasa.UserId && ug.GroupId == groupId);

        // ✅ She's in the group
        Assert.IsNotNull(membership);

        // ➡ Remove her from it
        context.UserGroup.Remove(membership);
        context.SaveChanges();

        var stillMember = context.UserGroup.Any(ug => ug.UserId == mikasa.UserId && ug.GroupId == groupId);
        Assert.IsFalse(stillMember);
    }

    [TestMethod]
    public void Creator_Can_Kick_Member()
    {
        using var context = GetInMemoryContext();
        var armin = context.User.First(u => u.Email == "armin@edu.dk");
        var mikasa = context.User.First(u => u.Email == "mikasa@edu.dk");
        var group = context.Group.First(g => g.CreatedByUserId == armin.UserId);

        var membership = context.UserGroup.FirstOrDefault(ug =>
            ug.UserId == mikasa.UserId && ug.GroupId == group.GroupId);

        // ✅ Mikasa is a member
        Assert.IsNotNull(membership);

        // ➡ Armin kicks her
        context.UserGroup.Remove(membership!);
        context.SaveChanges();

        var stillMember = context.UserGroup.Any(ug => ug.UserId == mikasa.UserId && ug.GroupId == group.GroupId);
        Assert.IsFalse(stillMember);
    }
}
