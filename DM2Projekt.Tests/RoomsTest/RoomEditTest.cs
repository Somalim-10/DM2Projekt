using DM2Projekt.Data;
using DM2Projekt.Models;
using DM2Projekt.Pages.Rooms;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace DM2Projekt.Tests.RoomsTest
{
    [TestClass]
    public class RoomEditTests
    {
        private DM2ProjektContext GetInMemoryContext()
        {
            var options = new DbContextOptionsBuilder<DM2ProjektContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new DM2ProjektContext(options);
        }

        [TestMethod]
        public async Task EditRoom_UpdatesRoomName_WhenValid()
        {
            // Arrange
            var context = GetInMemoryContext();

            var room = new Room { RoomId = 1, RoomName = "Originalt Navn", ImageUrl = "http://test.img" };
            context.Room.Add(room);
            await context.SaveChangesAsync();

            // Act
            var roomToEdit = await context.Room.FindAsync(1);
            roomToEdit.RoomName = "Opdateret Navn";
            context.Room.Update(roomToEdit);
            await context.SaveChangesAsync();

            // Assert
            var updatedRoom = await context.Room.FindAsync(1);
            Assert.AreEqual("Opdateret Navn", updatedRoom.RoomName);
        }

        [TestMethod]
        public async Task EditRoom_UpdatesImageUrl_WhenValidUrl()
        {
            // Arrange
            var context = GetInMemoryContext();

            var room = new Room { RoomId = 2, RoomName = "Testlokale", ImageUrl = "http://old.img" };
            context.Room.Add(room);
            await context.SaveChangesAsync();

            // Act
            var roomToEdit = await context.Room.FindAsync(2);
            roomToEdit.ImageUrl = "https://example.com/image.jpg"; // simulerer ny billede-URL
            context.Room.Update(roomToEdit);
            await context.SaveChangesAsync();

            // Assert
            var updatedRoom = await context.Room.FindAsync(2);
            Assert.AreEqual("https://example.com/image.jpg", updatedRoom.ImageUrl);
        }
        [TestMethod]
        public async Task EditRoom_ChangesRoomName_WhenUserIsAdmin()
        {
            // Arrange
            var context = GetInMemoryContext();

            var room = new Room { RoomId = 1, RoomName = "GamleNavn" };
            context.Room.Add(room);
            await context.SaveChangesAsync();

            var editModel = new EditModel(context);

            // Hent den eksisterende room fra context og ændr navn
            var roomFromDb = await context.Room.FindAsync(1);
            editModel.Room = roomFromDb!;
            editModel.Room.RoomName = "NytNavn";
            editModel.NewProfileImageUrl = null;

            editModel.ModelState.Clear(); // Vigtigt

            // Act
            var result = await editModel.OnPostAsync(testUserRole: "Admin", testUserId: 123);

            // Assert
            var updatedRoom = await context.Room.FindAsync(1);
            Assert.AreEqual("NytNavn", updatedRoom?.RoomName);
            Assert.IsInstanceOfType(result, typeof(RedirectToPageResult));
        }


    }
}
