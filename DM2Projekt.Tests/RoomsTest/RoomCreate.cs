using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.EntityFrameworkCore;
using DM2Projekt.Data;
using DM2Projekt.Models;
using DM2Projekt.Pages.Rooms;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Threading.Tasks;

namespace DM2Projekt.Tests.RoomsTest
{
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
            // Arrange
            var context = GetInMemoryContext();

            var existingRoom = new Room { RoomName = "TestRoom" };
            context.Room.Add(existingRoom);
            await context.SaveChangesAsync();

            var createModel = new CreateModel(context)
            {
                Room = new Room { RoomName = "TestRoom" }, // Samme navn som eksisterende rum
                NewProfileImageUrl = null
            };

            // Simulér at modellen er gyldig (ellers springer den fejl-check over)
            createModel.ModelState.Clear(); // sørg for den ikke er invalid

            // Act
            var result = await createModel.OnPostAsync(testUserRole: "Admin");

            // Assert
            Assert.IsInstanceOfType(result, typeof(PageResult));

            Assert.IsTrue(createModel.ModelState.ContainsKey("Room.RoomName"),
                "ModelState indeholder ikke 'Room.RoomName'");

            var modelStateEntry = createModel.ModelState["Room.RoomName"];
            Assert.IsNotNull(modelStateEntry, "ModelState entry for 'Room.RoomName' er null");
            Assert.IsTrue(modelStateEntry.Errors.Count > 0,
                "Ingen fejl fundet på Room.RoomName");

            Assert.AreEqual("Et rum med dette navn findes allerede.",
                modelStateEntry.Errors[0].ErrorMessage);
        }


    }
}
