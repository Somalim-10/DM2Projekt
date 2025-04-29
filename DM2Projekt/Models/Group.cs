using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace DM2Projekt.Models;

// group model
public class Group
{
    [Key]
    public int GroupId { get; set; }

    [Required]
    public string GroupName { get; set; }

    public int CreatedByUserId { get; set; }
    [ValidateNever] // prevents ModelState errors during form POST
    public User CreatedByUser { get; set; } = null!; // nav property

    // users in this group
    public ICollection<UserGroup> UserGroups { get; set; } = [];

    // bookings made by this group
    public ICollection<Booking> Bookings { get; set; } = [];
}
