using DM2Projekt.Data;
using DM2Projekt.Models;
using DM2Projekt.Pages.Rooms;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace DM2Projekt.Tests.RoomsTest;

[TestClass]
public class RoomTests
{
    private DM2ProjektContext GetInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<DM2ProjektContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new DM2ProjektContext(options);
    }

    [TestMethod]
    public async Task OnGetAsync_Filters_By_SearchTerm()
    {
        // Arrange
        var context = GetInMemoryContext();
        context.Room.AddRange(
            new Room { RoomId = 1, RoomName = "Lokale A", Building = Building.A, Floor = Floor.Ground, RoomType = RoomType.MeetingRoom },
            new Room { RoomId = 2, RoomName = "Lokale B", Building = Building.B, Floor = Floor.First, RoomType = RoomType.Classroom }
        );
        context.SaveChanges();

        var model = new IndexModel(context);

        // Simulate session (mock-style)
        var httpContext = new DefaultHttpContext();
        httpContext.Session = new TestSession(); // test session class below
        httpContext.Session.SetInt32("UserId", 123); // simulate logged in user
        model.PageContext = new Microsoft.AspNetCore.Mvc.RazorPages.PageContext
        {
            HttpContext = httpContext
        };

        // Act
        model.SearchTerm = "Lokale A";
        await model.OnGetAsync();

        // Assert
        Assert.AreEqual(1, model.Rooms.Count);
        Assert.AreEqual("Lokale A", model.Rooms[0].RoomName);
    }

    // fake session implementation so we can set stuff
    private class TestSession : ISession
    {
        private readonly Dictionary<string, byte[]> _sessionStorage = new();

        public IEnumerable<string> Keys => _sessionStorage.Keys;
        public string Id => Guid.NewGuid().ToString();
        public bool IsAvailable => true;

        public void Clear() => _sessionStorage.Clear();

        public Task CommitAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task LoadAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

        public void Remove(string key) => _sessionStorage.Remove(key);

        public void Set(string key, byte[] value) => _sessionStorage[key] = value;

        public bool TryGetValue(string key, out byte[] value) => _sessionStorage.TryGetValue(key, out value);
    }
}
