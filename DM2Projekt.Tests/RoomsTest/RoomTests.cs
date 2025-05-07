using DM2Projekt.Data;
using DM2Projekt.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using DM2Projekt.Pages.Rooms; // <-- for IndexModel
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DM2Projekt.Tests.RoomsTest
{
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

        // Testmetode for at teste FilterRooms
        [TestMethod]
        public void FilterRooms_Filters_By_RoomId()
        {
            // Arrange: Opretter en in-memory database og tilføjer nogle rum
            var context = GetInMemoryContext();
            context.Room.AddRange(
                new Room { RoomId = 1, RoomName = "Lokale A" },
                new Room { RoomId = 2, RoomName = "Lokale B" }
            );
            context.SaveChanges();

            var model = new IndexModel(context);

         
            var filtered = model.FilterRooms(context.Room.AsQueryable(), 1).ToList();

            Assert.AreEqual(1, filtered.Count); 
            Assert.AreEqual("Lokale A", filtered[0].RoomName); 
        }
    }
}
