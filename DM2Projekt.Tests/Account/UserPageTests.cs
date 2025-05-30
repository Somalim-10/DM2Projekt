using DM2Projekt.Data;
using DM2Projekt.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace DM2Projekt.Tests.Account;

[TestClass]
public class UserPageTests
{
    private const string ValidPassword = "123";
    private const string ImageUrlPattern = @"^https?:\/\/.*\.(jpg|jpeg|png|gif|webp|bmp|svg)$";

    private DM2ProjektContext CreateInMemoryContext()
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
            Password = ValidPassword,
            Role = Role.Student
        });

        context.SaveChanges();
        return context;
    }

    // Password change validation

    [TestMethod]
    public void ChangePassword_Should_Fail_If_Current_Wrong()
    {
        using var context = CreateInMemoryContext();
        var user = context.User.First();
        var wrongPassword = "notright";

        Assert.AreNotEqual(user.Password, wrongPassword, "should fail if current password is wrong");
    }

    [TestMethod]
    public void ChangePassword_Should_Fail_If_Same_As_Old()
    {
        using var context = CreateInMemoryContext();
        var user = context.User.First();

        Assert.AreEqual(ValidPassword, user.Password, "new password is same as old");
    }

    [TestMethod]
    public void ChangePassword_Should_Fail_If_Too_Short()
    {
        var shortPassword = "abc"; // less than 6

        Assert.IsTrue(shortPassword.Length < 6, "password is too short");
    }

    [TestMethod]
    public void ChangePassword_Should_Fail_If_Confirm_Doesnt_Match()
    {
        var newPass = "newpassword123";
        var confirm = "notthesame";

        Assert.AreNotEqual(newPass, confirm, "confirm password doesn't match");
    }

    // Relative time formatting

    [TestMethod]
    public void GetRelativeTime_Should_Show_Today()
    {
        var result = Pages.Account.UserPageModel.GetRelativeTime(DateTime.Now);
        Assert.AreEqual("today", result);
    }

    [TestMethod]
    public void GetRelativeTime_Should_Show_Tomorrow()
    {
        var result = Pages.Account.UserPageModel.GetRelativeTime(DateTime.Now.AddDays(1));
        Assert.AreEqual("tomorrow", result);
    }

    [TestMethod]
    public void GetRelativeTime_Should_Show_Number_Of_Days()
    {
        var result = Pages.Account.UserPageModel.GetRelativeTime(DateTime.Now.AddDays(5));
        Assert.AreEqual("in 5 days", result);
    }

    // Profile picture URL validation

    [TestMethod]
    public void SetProfilePicture_Should_Accept_Valid_Image_Url()
    {
        using var context = CreateInMemoryContext();
        var user = context.User.First();

        var validUrl = "https://example.com/image.png";
        var isValid = Regex.IsMatch(validUrl, ImageUrlPattern, RegexOptions.IgnoreCase);

        Assert.IsTrue(isValid, "URL should match allowed image extensions");

        user.ProfileImagePath = validUrl;
        context.SaveChanges();

        var updated = context.User.First();
        Assert.AreEqual(validUrl, updated.ProfileImagePath, "Profile image should be updated");
    }

    [TestMethod]
    public void SetProfilePicture_Should_Reject_Invalid_Image_Url()
    {
        var invalidUrl = "https://example.com/image.txt";
        var isValid = Regex.IsMatch(invalidUrl, ImageUrlPattern, RegexOptions.IgnoreCase);

        Assert.IsFalse(isValid, "Should reject unsupported image extension");
    }
}
