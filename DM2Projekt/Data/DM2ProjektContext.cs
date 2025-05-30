using Microsoft.EntityFrameworkCore;
using DM2Projekt.Models;

namespace DM2Projekt.Data;

// Sets up how everything connects in the database
public class DM2ProjektContext : DbContext
{
    public DM2ProjektContext(DbContextOptions<DM2ProjektContext> options)
        : base(options)
    {
    }

    // Tables
    public DbSet<Room> Room { get; set; } = default!;
    public DbSet<User> User { get; set; } = default!;
    public DbSet<Group> Group { get; set; } = default!;
    public DbSet<UserGroup> UserGroup { get; set; } = default!;
    public DbSet<Booking> Booking { get; set; } = default!;
    public DbSet<GroupInvitation> GroupInvitation { get; set; } = default!;

    // Used for setting up relations and other stuff related to the tables
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Save enums like RoomType as strings in the database
        modelBuilder.Entity<Room>(room =>
        {
            room.Property(r => r.RoomType).HasConversion<string>();
            room.Property(r => r.Building).HasConversion<string>();
            room.Property(r => r.Floor).HasConversion<string>();
        });

        // Save enum Role for users as strings
        modelBuilder.Entity<User>()
            .Property(u => u.Role)
            .HasConversion<string>();

        // Set up the many-to-many link between users and groups
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

        // A group knows who created it, but don't cascade delete to avoid SQL Server conflicts
        modelBuilder.Entity<Group>()
            .HasOne(g => g.CreatedByUser)
            .WithMany()
            .HasForeignKey(g => g.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        // When a group gets deleted, delete its bookings too
        modelBuilder.Entity<Booking>()
            .HasOne(b => b.Group)
            .WithMany(g => g.Bookings)
            .HasForeignKey(b => b.GroupId)
            .OnDelete(DeleteBehavior.Cascade);

        // Same for rooms, delete bookings if the room is removed
        modelBuilder.Entity<Booking>()
            .HasOne(b => b.Room)
            .WithMany(r => r.Bookings)
            .HasForeignKey(b => b.RoomId)
            .OnDelete(DeleteBehavior.Cascade);

        // Don’t automatically delete bookings if the user is deleted, that’s handled manually
        modelBuilder.Entity<Booking>()
            .HasOne(b => b.CreatedByUser)
            .WithMany(u => u.Bookings)
            .HasForeignKey(b => b.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        // Invitations are linked to both a group and a user
        modelBuilder.Entity<GroupInvitation>()
            .HasOne(i => i.Group)
            .WithMany()
            .HasForeignKey(i => i.GroupId);

        modelBuilder.Entity<GroupInvitation>()
            .HasOne(i => i.InvitedUser)
            .WithMany()
            .HasForeignKey(i => i.InvitedUserId);
    }
}
