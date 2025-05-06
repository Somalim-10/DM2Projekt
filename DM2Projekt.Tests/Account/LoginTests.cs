using DM2Projekt.Data;
using DM2Projekt.Models;
using DM2Projekt.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace DM2Projekt.Tests.Account;

[TestClass]
public class LoginTests
{
    private DM2ProjektContext GetContext()
    {
        var options = new DbContextOptionsBuilder<DM2ProjektContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var context = new DM2ProjektContext(options);

        context.User.Add(new User
        {
            FirstName = "Mikasa",
            LastName = "Ackerman",
            Email = "mikasa@edu.dk",
            Password = "123",
            Role = Role.Student
        });

        context.SaveChanges();
        return context;
    }

    [TestMethod]
    public void Login_With_Correct_Credentials_Should_Work()
    {
        using var context = GetContext();

        var user = context.User.FirstOrDefault(u =>
            u.Email == "mikasa@edu.dk" && u.Password == "123");

        Assert.IsNotNull(user, "login should work with correct creds");
    }

    [TestMethod]
    public void Login_With_Wrong_Password_Should_Fail()
    {
        using var context = GetContext();

        var user = context.User.FirstOrDefault(u =>
            u.Email == "mikasa@edu.dk" && u.Password == "wrong");

        Assert.IsNull(user, "login should fail with wrong password");
    }

    [TestMethod]
    public void Login_With_Unknown_Email_Should_Fail()
    {
        using var context = GetContext();

        var user = context.User.FirstOrDefault(u =>
            u.Email == "idontexist@edu.dk");

        Assert.IsNull(user, "shouldn't find unknown user");
    }
}
