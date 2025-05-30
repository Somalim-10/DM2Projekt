using DM2Projekt.Data;
using DM2Projekt.Models;
using Microsoft.EntityFrameworkCore;

namespace DM2Projekt.Tests.GroupFeatures;

[TestClass]
public class GroupInvitationTests
{
    // Creates a fresh DB and seeds some test data
    private DM2ProjektContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<DM2ProjektContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var context = new DM2ProjektContext(options);
        SeedTestData(context);
        return context;
    }

    // Adds 3 users, a group, and a pending invite
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
            GroupName = "Scouting Legion",
            CreatedByUserId = armin.UserId
        };
        context.Group.Add(group);
        context.SaveChanges();

        context.UserGroup.Add(new UserGroup { UserId = armin.UserId, GroupId = group.GroupId });

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
    public void Can_See_Pending_Invitations_For_Student()
    {
        using var context = CreateInMemoryContext();
        var levi = context.User.First(u => u.Email == "levi@edu.dk");

        var pending = context.GroupInvitation
            .Where(i => i.InvitedUserId == levi.UserId && i.IsAccepted == null)
            .ToList();

        Assert.AreEqual(1, pending.Count, "levi should have 1 pending invite");
    }

    [TestMethod]
    public void Accepting_Invite_Adds_User_To_Group()
    {
        using var context = CreateInMemoryContext();
        var levi = context.User.First(u => u.Email == "levi@edu.dk");

        var invite = context.GroupInvitation.First(i => i.InvitedUserId == levi.UserId);
        invite.IsAccepted = true;

        context.UserGroup.Add(new UserGroup
        {
            UserId = levi.UserId,
            GroupId = invite.GroupId
        });

        context.SaveChanges();

        var isMember = context.UserGroup.Any(ug =>
            ug.UserId == levi.UserId && ug.GroupId == invite.GroupId);

        Assert.IsTrue(isMember, "levi should now be a group member");
    }

    [TestMethod]
    public void Declining_Invite_Stores_Rejection()
    {
        using var context = CreateInMemoryContext();
        var levi = context.User.First(u => u.Email == "levi@edu.dk");

        var invite = context.GroupInvitation.First(i => i.InvitedUserId == levi.UserId);
        invite.IsAccepted = false;

        context.SaveChanges();

        var updated = context.GroupInvitation.First(i => i.InvitationId == invite.InvitationId);
        Assert.IsFalse(updated.IsAccepted, "invite should be marked as declined");
    }

    [TestMethod]
    public void Cannot_Reinvite_If_Pending_Exists()
    {
        using var context = CreateInMemoryContext();
        var group = context.Group.First();
        var levi = context.User.First(u => u.Email == "levi@edu.dk");

        var pendingExists = context.GroupInvitation.Any(i =>
            i.GroupId == group.GroupId &&
            i.InvitedUserId == levi.UserId &&
            i.IsAccepted == null);

        Assert.IsTrue(pendingExists, "levi already has a pending invite");

        var canReinvite = !pendingExists;
        Assert.IsFalse(canReinvite, "should not allow duplicate pending invite");
    }

    [TestMethod]
    public void Creator_Can_Cancel_Pending_Invite()
    {
        using var context = CreateInMemoryContext();

        var levi = context.User.First(u => u.Email == "levi@edu.dk");
        var invite = context.GroupInvitation
            .Include(i => i.Group)
            .First(i => i.InvitedUserId == levi.UserId);

        context.GroupInvitation.Remove(invite);
        context.SaveChanges();

        var stillExists = context.GroupInvitation.Any(i => i.InvitationId == invite.InvitationId);
        Assert.IsFalse(stillExists, "Invite should be deleted by creator");
    }

    [TestMethod]
    public void Only_Creator_Can_Cancel_Invite()
    {
        using var context = CreateInMemoryContext();
        var mikasa = context.User.First(u => u.Email == "mikasa@edu.dk");
        var invite = context.GroupInvitation
            .Include(i => i.Group)
            .First();

        var isCreator = invite.Group.CreatedByUserId == mikasa.UserId;

        Assert.IsFalse(isCreator, "Mikasa should not be able to cancel the invite");
    }
}
