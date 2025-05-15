using DM2Projekt.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DM2Projekt.Pages.Groups;
using Microsoft.EntityFrameworkCore;
using DM2Projekt.Models.Enums;
using DM2Projekt.Models;


namespace DM2Projekt.Tests.GroupFeatures;

[TestClass]

public class GroupDatabaseTest
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
    private void SeedFakeData(DM2ProjektContext context)
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
        context.SaveChanges();

        // send 1 invite to levi
        context.GroupInvitation.Add(new GroupInvitation
        {
            GroupId = group.GroupId,
            InvitedUserId = levi.UserId,
            SentAt = DateTime.Now,
            IsAccepted = null
        });

        context.SaveChanges();
    }

    public class TestSession : ISession
    {
        private readonly Dictionary<string, byte[]> _store = new();

        public bool IsAvailable => true;
        public string Id => Guid.NewGuid().ToString();
        public IEnumerable<string> Keys => _store.Keys;

        public void Clear() => _store.Clear();
        public void Remove(string key) => _store.Remove(key);
        public void Set(string key, byte[] value) => _store[key] = value;
        public bool TryGetValue(string key, out byte[] value) => _store.TryGetValue(key, out value);

        public Task CommitAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task LoadAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
    }
    private CreateModel CreatePageModel(DM2ProjektContext context, string role, int? userId)
    {
        // Create a fake HttpContext and session
        var httpContext = new DefaultHttpContext();
        httpContext.Session = new TestSession(); // Fake session (defined below)

        // Set session values
        if (role != null)
            httpContext.Session.SetString("UserRole", role);
        if (userId.HasValue)
            httpContext.Session.SetInt32("UserId", userId.Value);

        // Build page context
        var pageContext = new PageContext
        {
            HttpContext = httpContext
        };

        // Create instance of CreateModel and inject page context
        var pageModel = new CreateModel(context)
        {
            PageContext = pageContext
        };

        return pageModel;
    }


    [TestMethod]
    public async Task Post_Redirects_If_User_Not_Found()
    {
        var context = GetInMemoryContext();

        // bruger-ID eksisterer ikke
        var fakeUserId = 999;
        var page = CreatePageModel(context, "Student", fakeUserId);
        page.Group = new Group { GroupName = "GhostGroup" };

        var result = await page.OnPostAsync();

        Assert.IsInstanceOfType(result, typeof(RedirectToPageResult));
        Assert.AreEqual("/Login", ((RedirectToPageResult)result).PageName);
    }
    [TestMethod]
    public async Task Post_Allows_Duplicate_GroupNames_By_Different_Users()
    {
        var context = GetInMemoryContext();

        // create second user (must include LastName!)
        var otherUser = new User
        {
            FirstName = "Jean",
            LastName = "Kirstein", // 🔧 Required field that caused your error
            Email = "jean@edu.dk",
            Password = "123",
            Role = Role.Student
        };

        context.User.Add(otherUser);
        context.SaveChanges();

        // First group created by 'otherUser'
        context.Group.Add(new Group
        {
            GroupName = "DoublesAllowed",
            CreatedByUserId = otherUser.UserId
        });
        context.SaveChanges();

        // Get test user from seeded data (e.g. Eren or Armin)
        var currentUser = context.User.First(u => u.Email == "armin@edu.dk");

        // Act: try to create a group with same name by a different user
        var page = CreatePageModel(context, "Student", currentUser.UserId);
        page.Group = new Group { GroupName = "DoublesAllowed" };

        var result = await page.OnPostAsync();

        
        if (result is PageResult)
        {
            // Print ModelState error to the consol
            foreach (var error in page.ModelState.Values.SelectMany(v => v.Errors))
            {
                Console.WriteLine("ModelState Error: " + error.ErrorMessage);
            }

            Assert.Fail("Expected RedirectToPageResult, but got PageResult. Possible validation or logic failure.");
        }

        Assert.IsInstanceOfType(result, typeof(RedirectToPageResult));
        Assert.AreEqual("./Index", ((RedirectToPageResult)result).PageName);
    }
    public async Task Post_Adds_User_To_Own_Group()
    {
        var context = GetInMemoryContext();
        var user = context.User.First();

        var page = CreatePageModel(context, "Student", user.UserId);
        page.Group = new Group { GroupName = "SelfJoinGroup" };

        await page.OnPostAsync();

        var group = context.Group.FirstOrDefault(g => g.GroupName == "SelfJoinGroup");
        var isMember = context.UserGroup.Any(ug => ug.UserId == user.UserId && ug.GroupId == group.GroupId);

        Assert.IsTrue(isMember, "User should be added to their own group");
    }
    [TestMethod]
    public async Task Post_Returns_Page_If_ModelState_Invalid()
    {
        var context = GetInMemoryContext();
        var user = context.User.First();

        var page = CreatePageModel(context, "Student", user.UserId);

        // tving fejl
        page.ModelState.AddModelError("Group.GroupName", "Required");

        var result = await page.OnPostAsync();

        Assert.IsInstanceOfType(result, typeof(PageResult));
        Assert.IsTrue(page.ModelState.ErrorCount > 0);
    }
  
    [TestMethod]
    public async Task Post_Redirects_If_Session_Missing_UserId()
    {
        var context = GetInMemoryContext();
        var page = CreatePageModel(context, "Student", null); // no user ID
        page.Group = new Group { GroupName = "SessionlessGroup" };

        var result = await page.OnPostAsync();

        Assert.IsInstanceOfType(result, typeof(RedirectToPageResult));
        Assert.AreEqual("/Login", ((RedirectToPageResult)result).PageName);
    }
}

