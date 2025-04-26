using System.ComponentModel.DataAnnotations;
using DM2Projekt.Models.Enums;

namespace DM2Projekt.Models;

// user model
public class User
{
    [Key]
    public int UserId { get; set; }

    [Required]
    public string FirstName { get; set; }

    [Required]
    public string LastName { get; set; }

    [Required, EmailAddress]
    public string Email { get; set; }

    [Required]
    public Role Role { get; set; } // student or teacher

    [Required]
    public string Password { get; set; }

    // groups this user is in
    public ICollection<UserGroup> UserGroups { get; set; } = [];

    // bookings this user made
    public ICollection<Booking> Bookings { get; set; } = [];
}
