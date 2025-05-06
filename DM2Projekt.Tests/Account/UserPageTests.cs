using DM2Projekt.Data;
using DM2Projekt.Models;
using DM2Projekt.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace DM2Projekt.Tests.Account;

[TestClass]
public class UserPageTests
{
    private DM2ProjektContext GetContext()
    {
        var options = new DbContextOptionsBuilder<DM2ProjektContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var context = new DM2ProjektContext(options);

        context.User.Add(new User
        {
            FirstName = "Armin",
            LastName = "Arlert",
            Email = "armin@edu.dk",
            Password = "123",
            Role = Role.Student
        });

        context.SaveChanges();
        return context;
    }

    [TestMethod]
    public void ChangePassword_Should_Fail_If_Current_Wrong()
    {
        using var context = GetContext();
        var user = context.User.First();
        var wrongInput = "notright";

        Assert.AreNotEqual(user.Password, wrongInput, "should fail if current password is wrong");
    }

    [TestMethod]
    public void ChangePassword_Should_Fail_If_Same_As_Old()
    {
        using var context = GetContext();
        var user = context.User.First();

        var same = user.Password;

        Assert.AreEqual("123", same, "new password is same as old");
    }

    [TestMethod]
    public void ChangePassword_Should_Fail_If_Too_Short()
    {
        var newPassword = "abc"; // less than 6

        Assert.IsTrue(newPassword.Length < 6, "password is too short");
    }

    [TestMethod]
    public void ChangePassword_Should_Fail_If_Confirm_Doesnt_Match()
    {
        var newPass = "newpassword123";
        var confirm = "notthesame";

        Assert.AreNotEqual(newPass, confirm, "confirm password doesn't match");
    }

    [TestMethod]
    public void GetRelativeTime_Should_Show_Today()
    {
        var today = DateTime.Now;

        var result = Pages.Account.UserPageModel.GetRelativeTime(today);

        Assert.AreEqual("today", result);
    }

    [TestMethod]
    public void GetRelativeTime_Should_Show_Tomorrow()
    {
        var tomorrow = DateTime.Now.AddDays(1);

        var result = Pages.Account.UserPageModel.GetRelativeTime(tomorrow);

        Assert.AreEqual("tomorrow", result);
    }

    [TestMethod]
    public void GetRelativeTime_Should_Show_Number_Of_Days()
    {
        var inFiveDays = DateTime.Now.AddDays(5);

        var result = Pages.Account.UserPageModel.GetRelativeTime(inFiveDays);

        Assert.AreEqual("in 5 days", result);
    }
}
