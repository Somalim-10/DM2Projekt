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

    // tables in database
    public DbSet<Room> Room { get; set; } = default!;
    public DbSet<User> User { get; set; } = default!;
    public DbSet<Group> Group { get; set; } = default!;
    public DbSet<UserGroup> UserGroup { get; set; } = default!;
    public DbSet<Booking> Booking { get; set; } = default!;

    // setting up rules for tables
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // always call base first
        base.OnModelCreating(modelBuilder);

        // tell EF: RoomType is saved as text in database
        modelBuilder.Entity<Room>()
            .Property(r => r.RoomType)
            .HasConversion<string>();

        // tell EF: Role is saved as text in database
        modelBuilder.Entity<User>()
            .Property(u => u.Role)
            .HasConversion<string>();

        // set primary key for UserGroup (combo of UserId + GroupId)
        modelBuilder.Entity<UserGroup>()
            .HasKey(ug => new { ug.UserId, ug.GroupId });

        // setup relation: UserGroup -> User (many-to-one)
        modelBuilder.Entity<UserGroup>()
            .HasOne(ug => ug.User)
            .WithMany(u => u.UserGroups)
            .HasForeignKey(ug => ug.UserId);

        // setup relation: UserGroup -> Group (many-to-one)
        modelBuilder.Entity<UserGroup>()
            .HasOne(ug => ug.Group)
            .WithMany(g => g.UserGroups)
            .HasForeignKey(ug => ug.GroupId);

        // setup relation: Booking -> CreatedByUser (many-to-one)
        modelBuilder.Entity<Booking>()
            .HasOne(b => b.CreatedByUser)
            .WithMany(u => u.Bookings)
            .HasForeignKey(b => b.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict); // don't auto delete user bookings

        // setup relation: Booking -> Group (many-to-one)
        modelBuilder.Entity<Booking>()
            .HasOne(b => b.Group)
            .WithMany(g => g.Bookings)
            .HasForeignKey(b => b.GroupId)
            .OnDelete(DeleteBehavior.Restrict); // don't auto delete group bookings

        // setup relation: Booking -> Room (many-to-one)
        modelBuilder.Entity<Booking>()
            .HasOne(b => b.Room)
            .WithMany(r => r.Bookings)
            .HasForeignKey(b => b.RoomId)
            .OnDelete(DeleteBehavior.Restrict); // don't auto delete room bookings
    }
}
