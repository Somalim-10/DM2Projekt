using Microsoft.EntityFrameworkCore;
using DM2Projekt.Models;

namespace DM2Projekt.Data;

// this is the main class for database stuff
public class DM2ProjektContext : DbContext
{
    public DM2ProjektContext(DbContextOptions<DM2ProjektContext> options)
        : base(options)
    {
    }

    // all our tables
    public DbSet<Room> Room { get; set; } = default!;
    public DbSet<User> User { get; set; } = default!;
    public DbSet<Group> Group { get; set; } = default!;
    public DbSet<UserGroup> UserGroup { get; set; } = default!;
    public DbSet<Booking> Booking { get; set; } = default!;
    public DbSet<GroupInvitation> GroupInvitation { get; set; } = default!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // --- store enums as strings in DB ---
        modelBuilder.Entity<Room>()
            .Property(r => r.RoomType)
            .HasConversion<string>();

        modelBuilder.Entity<Room>()
            .Property(r => r.Building)
            .HasConversion<string>();

        modelBuilder.Entity<Room>()
            .Property(r => r.Floor)
            .HasConversion<string>();

        modelBuilder.Entity<User>()
            .Property(u => u.Role)
            .HasConversion<string>();

        // --- composite key for UserGroup (user + group) ---
        modelBuilder.Entity<UserGroup>()
            .HasKey(ug => new { ug.UserId, ug.GroupId });

        // user <-> usergroups
        modelBuilder.Entity<UserGroup>()
            .HasOne(ug => ug.User)
            .WithMany(u => u.UserGroups)
            .HasForeignKey(ug => ug.UserId);

        // group <-> usergroups
        modelBuilder.Entity<UserGroup>()
            .HasOne(ug => ug.Group)
            .WithMany(g => g.UserGroups)
            .HasForeignKey(ug => ug.GroupId);

        // booking made by user
        modelBuilder.Entity<Booking>()
            .HasOne(b => b.CreatedByUser)
            .WithMany(u => u.Bookings)
            .HasForeignKey(b => b.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        // booking belongs to a group
        modelBuilder.Entity<Booking>()
            .HasOne(b => b.Group)
            .WithMany(g => g.Bookings)
            .HasForeignKey(b => b.GroupId)
            .OnDelete(DeleteBehavior.Restrict);

        // booking belongs to a room — if room is deleted, kill the bookings too
        modelBuilder.Entity<Booking>()
            .HasOne(b => b.Room)
            .WithMany(r => r.Bookings)
            .HasForeignKey(b => b.RoomId)
            .OnDelete(DeleteBehavior.Cascade); // remove all active bookings attached to this room

        // group has a creator (user)
        modelBuilder.Entity<Group>()
            .HasOne(g => g.CreatedByUser)
            .WithMany()
            .HasForeignKey(g => g.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        // group invitation stuff
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
