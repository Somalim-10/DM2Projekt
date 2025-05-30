using System.ComponentModel.DataAnnotations;

namespace DM2Projekt.Models;

public class User
{
    [Key]
    public int UserId { get; set; }

    [Required]
    public string FirstName { get; set; }

    [Required]
    public string LastName { get; set; }

    // optional. used for avatar
    public string? ProfileImagePath { get; set; }

    [Required, EmailAddress]
    public string Email { get; set; }

    [Required]
    public Role Role { get; set; }

    [Required]
    public string Password { get; set; }

    // which groups user is in
    public ICollection<UserGroup> UserGroups { get; set; } = [];

    // bookings user created
    public ICollection<Booking> Bookings { get; set; } = [];
}
