using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DM2Projekt.Models.Enums;

namespace DM2Projekt.Models;

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
    public Role Role { get; set; }

    public ICollection<UserGroup> UserGroups { get; set; } = new List<UserGroup>();
}
