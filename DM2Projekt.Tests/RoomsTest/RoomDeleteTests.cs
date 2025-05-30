using DM2Projekt.Data;
using DM2Projekt.Models;
using Microsoft.EntityFrameworkCore;

namespace DM2Projekt.Tests.RoomsTest;

[TestClass]
public class RoomDeleteTests
{
    private DM2ProjektContext GetInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<DM2ProjektContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new DM2ProjektContext(options);
    }

    [TestMethod]
    public async Task DeleteRoom_RemovesRoom_WhenExists()
    {
        var context = GetInMemoryContext();

        // Create a test room
        var room = new Room { RoomId = 1, RoomName = "TestLokale" };
        context.Room.Add(room);
        await context.SaveChangesAsync();

        // Now try deleting it
        var roomToDelete = await context.Room.FindAsync(1);
        context.Room.Remove(roomToDelete);
        await context.SaveChangesAsync();

        // Should be gone
        var roomExists = await context.Room.FindAsync(1);
        Assert.IsNull(roomExists);
    }
}
