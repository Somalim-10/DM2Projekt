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

        public DbSet<Room> Room { get; set; } = default!;
        public DbSet<User> User { get; set; } = default!;
        public DbSet<Group> Group { get; set; } = default!;
        public DbSet<Smartboard> Smartboard { get; set; } = default!;
        public DbSet<UserGroup> UserGroup { get; set; } = default!;

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

            modelBuilder.Entity<UserGroup>()
    .HasKey(ug => new { ug.UserId, ug.GroupId });

            modelBuilder.Entity<UserGroup>()
                .HasOne(ug => ug.User)
                .WithMany(u => u.UserGroups)
                .HasForeignKey(ug => ug.UserId);

            modelBuilder.Entity<UserGroup>()
                .HasOne(ug => ug.Group)
                .WithMany(g => g.UserGroups)
                .HasForeignKey(ug => ug.GroupId);
        }
        public DbSet<DM2Projekt.Models.Booking> Booking { get; set; } = default!;
    }
}
