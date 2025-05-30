using Microsoft.EntityFrameworkCore;
using DM2Projekt.Data;
using DM2Projekt.Models;

namespace DM2Projekt.Tests.Users;

[TestClass]
public class UserCrudTests
{
    // helper that gives us a clean in-memory database for each test
    private DM2ProjektContext GetContext()
    {
        var options = new DbContextOptionsBuilder<DM2ProjektContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var context = new DM2ProjektContext(options);

        // toss in a basic admin user we can mess with
        context.User.Add(new User
        {
            FirstName = "Test",
            LastName = "User",
            Email = "test@example.com",
            Password = "123456",
            Role = Role.Admin
        });

        context.SaveChanges();
        return context;
    }

    [TestMethod]
    public void CreateUser_ShouldAddUser()
    {
        using var context = GetContext();

        var newUser = new User
        {
            FirstName = "Eren",
            LastName = "Yeager",
            Email = "eren@paradis.com",
            Password = "freedom",
            Role = Role.Student
        };

        context.User.Add(newUser);
        context.SaveChanges();

        var found = context.User.FirstOrDefault(u => u.Email == "eren@paradis.com");

        Assert.IsNotNull(found);
        Assert.AreEqual("Eren", found.FirstName);
    }

    [TestMethod]
    public void EditUser_ShouldUpdateInfo()
    {
        using var context = GetContext();

        var user = context.User.First();
        user.FirstName = "Updated";
        user.Email = "updated@example.com";

        context.SaveChanges();

        var updated = context.User.First();
        Assert.AreEqual("Updated", updated.FirstName);
        Assert.AreEqual("updated@example.com", updated.Email);
    }

    [TestMethod]
    public void DeleteUser_ShouldRemoveUser()
    {
        using var context = GetContext();

        var user = context.User.First();
        context.User.Remove(user);
        context.SaveChanges();

        var remaining = context.User.ToList();
        Assert.AreEqual(0, remaining.Count, "User should be gone");
    }

    [TestMethod]
    public void GetUser_ShouldReturnCorrectUser()
    {
        using var context = GetContext();

        var user = context.User.FirstOrDefault(u => u.Email == "test@example.com");

        Assert.IsNotNull(user);
        Assert.AreEqual("Test", user.FirstName);
        Assert.AreEqual(Role.Admin, user.Role);
    }

    [TestMethod]
    public void CreateUser_ShouldFail_WithoutEmail()
    {
        using var context = GetContext();

        var user = new User
        {
            FirstName = "NoEmailGuy",
            Password = "123",
            Role = Role.Teacher
        };

        context.User.Add(user);

        try
        {
            context.SaveChanges();
            Assert.Fail("This should blow up — Email is required");
        }
        catch (DbUpdateException)
        {
            // good. model validation is working
        }
    }

    [TestMethod]
    public void CreateUser_ShouldFail_WithoutPassword()
    {
        using var context = GetContext();

        var user = new User
        {
            FirstName = "NoPass",
            Email = "fail@example.com",
            Role = Role.Teacher
        };

        context.User.Add(user);

        try
        {
            context.SaveChanges();
            Assert.Fail("This should crash — missing password");
        }
        catch (DbUpdateException)
        {
            // good. password can't be null
        }
    }

    [TestMethod]
    public void Role_ShouldContainValidValues()
    {
        var roles = Enum.GetValues(typeof(Role)).Cast<Role>().ToList();

        Assert.IsTrue(roles.Contains(Role.Admin));
        Assert.IsTrue(roles.Contains(Role.Teacher));
        Assert.IsTrue(roles.Contains(Role.Student));
    }
}
