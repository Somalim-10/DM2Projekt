using Microsoft.EntityFrameworkCore;
using DM2Projekt.Models;

namespace DM2Projekt.Data;

// database context – holds all our data tables
public class DM2ProjektContext : DbContext
{
    public DM2ProjektContext(DbContextOptions<DM2ProjektContext> options)
        : base(options)
    {
    }

    // these are mapped to DB tables
    public DbSet<Room> Room { get; set; } = default!;
    public DbSet<User> User { get; set; } = default!;
    public DbSet<Group> Group { get; set; } = default!;
    public DbSet<UserGroup> UserGroup { get; set; } = default!;
    public DbSet<Booking> Booking { get; set; } = default!;
    public DbSet<GroupInvitation> GroupInvitation { get; set; } = default!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // save enums as strings in DB – easier to read/debug
        modelBuilder.Entity<Room>(room =>
        {
            room.Property(r => r.RoomType).HasConversion<string>();
            room.Property(r => r.Building).HasConversion<string>();
            room.Property(r => r.Floor).HasConversion<string>();
        });

        modelBuilder.Entity<User>()
            .Property(u => u.Role)
            .HasConversion<string>();

        // link table – combo of user+group is the key
        modelBuilder.Entity<UserGroup>()
            .HasKey(ug => new { ug.UserId, ug.GroupId });

        // connect user <-> usergroups
        modelBuilder.Entity<UserGroup>()
            .HasOne(ug => ug.User)
            .WithMany(u => u.UserGroups)
            .HasForeignKey(ug => ug.UserId);

        // connect group <-> usergroups
        modelBuilder.Entity<UserGroup>()
            .HasOne(ug => ug.Group)
            .WithMany(g => g.UserGroups)
            .HasForeignKey(ug => ug.GroupId);

        // keep bookings even if user/group gets deleted
        modelBuilder.Entity<Booking>()
            .HasOne(b => b.CreatedByUser)
            .WithMany(u => u.Bookings)
            .HasForeignKey(b => b.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Booking>()
            .HasOne(b => b.Group)
            .WithMany(g => g.Bookings)
            .HasForeignKey(b => b.GroupId)
            .OnDelete(DeleteBehavior.Restrict);

        // if a room is deleted, delete its bookings too
        modelBuilder.Entity<Booking>()
            .HasOne(b => b.Room)
            .WithMany(r => r.Bookings)
            .HasForeignKey(b => b.RoomId)
            .OnDelete(DeleteBehavior.Cascade);

        // group creator (user) – don't remove groups if user is deleted
        modelBuilder.Entity<Group>()
            .HasOne(g => g.CreatedByUser)
            .WithMany()
            .HasForeignKey(g => g.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        // handle invites
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
