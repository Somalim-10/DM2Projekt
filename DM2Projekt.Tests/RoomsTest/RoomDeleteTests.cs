using DM2Projekt.Data;
using DM2Projekt.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DM2Projekt.Tests.RoomsTest
{
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
            // Arrange
            var context = GetInMemoryContext();

            var room = new Room { RoomId = 1, RoomName = "TestLokale" };
            context.Room.Add(room);
            await context.SaveChangesAsync();

            // Act
            var roomToDelete = await context.Room.FindAsync(1);
            context.Room.Remove(roomToDelete);
            await context.SaveChangesAsync();

            // Assert
            var roomExists = await context.Room.FindAsync(1);
            Assert.IsNull(roomExists);
        }
    }
}
