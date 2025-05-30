using DM2Projekt.Data;
using DM2Projekt.Models;
using DM2Projekt.Pages.Groups;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace DM2Projekt.Tests.GroupFeatures;

[TestClass]
public class GroupDatabaseTest
{
    // Sets up a fresh in-memory DB with 2 test users
    private DM2ProjektContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<DM2ProjektContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var context = new DM2ProjektContext(options);
        context.User.AddRange(
            new User { FirstName = "Armin", LastName = "Arlert", Email = "armin@edu.dk", Password = "123", Role = Role.Student },
            new User { FirstName = "Mikasa", LastName = "Ackerman", Email = "mikasa@edu.dk", Password = "123", Role = Role.Student }
        );
        context.SaveChanges();

        return context;
    }

    // Fake session storage (so .Session works during tests)
    private class TestSession : ISession
    {
        private readonly Dictionary<string, byte[]> _store = new();
        public bool IsAvailable => true;
        public string Id => Guid.NewGuid().ToString();
        public IEnumerable<string> Keys => _store.Keys;
        public void Clear() => _store.Clear();
        public void Remove(string key) => _store.Remove(key);
        public void Set(string key, byte[] value) => _store[key] = value;
        public bool TryGetValue(string key, out byte[] value) => _store.TryGetValue(key, out value);
        public Task CommitAsync(CancellationToken _) => Task.CompletedTask;
        public Task LoadAsync(CancellationToken _) => Task.CompletedTask;
    }

    // Sets up the Create page with optional role + userId in session
    private CreateModel CreatePageModel(DM2ProjektContext context, string? role = null, int? userId = null)
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Session = new TestSession();

        if (role != null)
            httpContext.Session.SetString("UserRole", role);
        if (userId.HasValue)
            httpContext.Session.SetInt32("UserId", userId.Value);

        return new CreateModel(context)
        {
            PageContext = new PageContext { HttpContext = httpContext }
        };
    }

    [TestMethod]
    public async Task Redirects_If_Session_Missing_UserId()
    {
        var context = CreateInMemoryContext();
        var page = CreatePageModel(context, "Student");
        page.Group = new Group { GroupName = "LonelyGroup" };

        var result = await page.OnPostAsync();

        Assert.IsInstanceOfType(result, typeof(RedirectToPageResult));
        Assert.AreEqual("/Login", ((RedirectToPageResult)result).PageName);
    }

    [TestMethod]
    public async Task Redirects_If_User_Not_Found()
    {
        var context = CreateInMemoryContext();
        var page = CreatePageModel(context, "Student", 999); // no such user
        page.Group = new Group { GroupName = "GhostGroup" };

        var result = await page.OnPostAsync();

        Assert.IsInstanceOfType(result, typeof(RedirectToPageResult));
        Assert.AreEqual("/Login", ((RedirectToPageResult)result).PageName);
    }

    [TestMethod]
    public async Task Creates_Group_And_Adds_User_To_It()
    {
        var context = CreateInMemoryContext();
        var user = context.User.First();

        var page = CreatePageModel(context, "Student", user.UserId);
        page.Group = new Group { GroupName = "NewGroup" };

        var result = await page.OnPostAsync();

        var group = context.Group.FirstOrDefault(g => g.GroupName == "NewGroup");
        var isMember = context.UserGroup.Any(ug => ug.UserId == user.UserId && ug.GroupId == group.GroupId);

        Assert.IsInstanceOfType(result, typeof(RedirectToPageResult));
        Assert.AreEqual("./Index", ((RedirectToPageResult)result).PageName);
        Assert.IsNotNull(group, "group should be saved");
        Assert.IsTrue(isMember, "user should be added to group");
    }

    [TestMethod]
    public async Task Allows_Duplicate_GroupName_By_Different_Users()
    {
        var context = CreateInMemoryContext();
        var user1 = context.User.First();
        var user2 = context.User.Last();

        // First user creates a group
        context.Group.Add(new Group { GroupName = "SameName", CreatedByUserId = user1.UserId });
        context.SaveChanges();

        // Second user creates a group with same name
        var page = CreatePageModel(context, "Student", user2.UserId);
        page.Group = new Group { GroupName = "SameName" };

        var result = await page.OnPostAsync();

        Assert.IsInstanceOfType(result, typeof(RedirectToPageResult));
        Assert.AreEqual("./Index", ((RedirectToPageResult)result).PageName);
    }

    [TestMethod]
    public async Task Fails_When_ModelState_Invalid()
    {
        var context = CreateInMemoryContext();
        var user = context.User.First();

        var page = CreatePageModel(context, "Student", user.UserId);
        page.ModelState.AddModelError("Group.GroupName", "Required");

        var result = await page.OnPostAsync();

        Assert.IsInstanceOfType(result, typeof(PageResult));
        Assert.IsTrue(page.ModelState.ErrorCount > 0);
    }
}
