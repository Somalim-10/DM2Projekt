using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using DM2Projekt.Models;

namespace DM2Projekt.Data
{
    public class DM2ProjektContext : DbContext
    {
        public DM2ProjektContext(DbContextOptions<DM2ProjektContext> options)
            : base(options)
        {
        }

        public DbSet<DM2Projekt.Models.Room> Room { get; set; } = default!;
        public DbSet<DM2Projekt.Models.User> User { get; set; } = default!;
        public DbSet<DM2Projekt.Models.Group> Group { get; set; } = default!;
        public DbSet<DM2Projekt.Models.Smartboard> Smartboard { get; set; } = default!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Konverter enum til string
            modelBuilder.Entity<Room>()
                .Property(r => r.RoomType)
                .HasConversion<string>();

            modelBuilder.Entity<User>()
                .Property(u => u.Role)
                .HasConversion<string>();
        }
        public DbSet<DM2Projekt.Models.Booking> Booking { get; set; } = default!;
    }
}
