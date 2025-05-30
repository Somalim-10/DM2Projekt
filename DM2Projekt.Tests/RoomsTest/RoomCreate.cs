using Microsoft.EntityFrameworkCore;
using DM2Projekt.Data;
using DM2Projekt.Models;
using DM2Projekt.Pages.Rooms;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DM2Projekt.Tests.RoomsTest;

[TestClass]
public class RoomCreate
{
    private DM2ProjektContext GetInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<DM2ProjektContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new DM2ProjektContext(options);
    }

    [TestMethod]
    public async Task CreateRoom_ReturnsPage_WhenRoomNameAlreadyExists()
    {
        var context = GetInMemoryContext();

        // Add a room that already exists
        var existingRoom = new Room { RoomName = "TestRoom" };
        context.Room.Add(existingRoom);
        await context.SaveChangesAsync();

        var createModel = new CreateModel(context)
        {
            Room = new Room { RoomName = "TestRoom" }, // Same name as the existing one
            NewProfileImageUrl = null
        };

        createModel.ModelState.Clear(); // make sure model state is valid

        var result = await createModel.OnPostAsync(testUserRole: "Admin");

        Assert.IsInstanceOfType<PageResult>(result);

        Assert.IsTrue(createModel.ModelState.ContainsKey("Room.RoomName"));
        var modelStateEntry = createModel.ModelState["Room.RoomName"];
        Assert.IsNotNull(modelStateEntry);
        Assert.IsTrue(modelStateEntry.Errors.Count > 0);
        Assert.AreEqual("Et rum med dette navn findes allerede.", modelStateEntry.Errors[0].ErrorMessage);
    }
}
