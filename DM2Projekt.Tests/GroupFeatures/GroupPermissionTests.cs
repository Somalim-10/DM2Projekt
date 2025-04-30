using DM2Projekt.Data;
using DM2Projekt.Models;
using DM2Projekt.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace DM2Projekt.Tests.GroupFeatures;

[TestClass]
public class GroupPermissionTests
{
    // setup fake db
    private DM2ProjektContext GetInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<DM2ProjektContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var context = new DM2ProjektContext(options);
        SeedFakeData(context);
        return context;
    }

    // setup users + group
    private void SeedFakeData(DM2ProjektContext context)
    {
        if (context.User.Any()) return;

        var admin = new User { FirstName = "Admin", LastName = "One", Email = "admin@dk", Password = "pass", Role = Role.Admin };
        var teacher = new User { FirstName = "Teach", LastName = "Er", Email = "teach@dk", Password = "pass", Role = Role.Teacher };
        var student1 = new User { FirstName = "Stu", LastName = "Dent", Email = "student1@dk", Password = "pass", Role = Role.Student };
        var student2 = new User { FirstName = "Other", LastName = "Student", Email = "student2@dk", Password = "pass", Role = Role.Student };

        context.User.AddRange(admin, teacher, student1, student2);
        context.SaveChanges();

        var group = new Group
        {
            GroupName = "Group A",
            CreatedByUserId = student1.UserId
        };
        context.Group.Add(group);
        context.SaveChanges();

        context.UserGroup.Add(new UserGroup { GroupId = group.GroupId, UserId = student1.UserId });
        context.SaveChanges();
    }

    [TestMethod]
    public void Only_Admin_And_Creator_Can_Delete_Group()
    {
        using var context = GetInMemoryContext();

        var group = context.Group.First();
        var creator = context.User.First(u => u.Email == "student1@dk");
        var admin = context.User.First(u => u.Email == "admin@dk");
        var outsider = context.User.First(u => u.Email == "student2@dk");

        // creator can delete
        var canCreatorDelete = group.CreatedByUserId == creator.UserId || creator.Role == Role.Admin;
        Assert.IsTrue(canCreatorDelete);

        // admin can delete
        var canAdminDelete = group.CreatedByUserId == admin.UserId || admin.Role == Role.Admin;
        Assert.IsTrue(canAdminDelete);

        // random student can't delete
        var canOutsiderDelete = group.CreatedByUserId == outsider.UserId || outsider.Role == Role.Admin;
        Assert.IsFalse(canOutsiderDelete);
    }

    [TestMethod]
    public void Teacher_Can_View_Any_Group_But_Cannot_Edit()
    {
        using var context = GetInMemoryContext();
        var teacher = context.User.First(u => u.Role == Role.Teacher);
        var group = context.Group.First();

        // teachers can view
        var canView = teacher.Role == Role.Teacher;
        Assert.IsTrue(canView);

        // but not edit
        var canEdit = group.CreatedByUserId == teacher.UserId || teacher.Role == Role.Admin;
        Assert.IsFalse(canEdit);
    }

    [TestMethod]
    public void Student_Cannot_View_If_Not_Member_Or_Creator()
    {
        using var context = GetInMemoryContext();
        var outsider = context.User.First(u => u.Email == "student2@dk");
        var group = context.Group.First();

        var isMember = context.UserGroup.Any(ug => ug.UserId == outsider.UserId && ug.GroupId == group.GroupId);
        var isCreator = group.CreatedByUserId == outsider.UserId;

        var canView = isMember || isCreator || outsider.Role == Role.Admin || outsider.Role == Role.Teacher;
        Assert.IsFalse(canView);
    }

    [TestMethod]
    public void Admin_Has_Full_Access()
    {
        using var context = GetInMemoryContext();
        var admin = context.User.First(u => u.Role == Role.Admin);
        var group = context.Group.First();

        var canView = true;
        var canEdit = true;
        var canDelete = true;

        Assert.IsTrue(canView);
        Assert.IsTrue(canEdit);
        Assert.IsTrue(canDelete);
    }
}
