using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace DM2Projekt.Models;

public class Group
{
    [Key]
    public int GroupId { get; set; }

    [Required]
    public string GroupName { get; set; }

    public int CreatedByUserId { get; set; }

    [ValidateNever] // not posted from forms, skip validation
    public User CreatedByUser { get; set; } = null!;

    // users in the group
    public ICollection<UserGroup> UserGroups { get; set; } = [];

    // bookings made by the group
    public ICollection<Booking> Bookings { get; set; } = [];
}
