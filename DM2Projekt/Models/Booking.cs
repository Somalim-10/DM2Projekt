using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace DM2Projekt.Models;

// booking model
public class Booking
{
    [Key]
    public int BookingId { get; set; }

    [Required]
    public int GroupId { get; set; }

    [Required]
    public int RoomId { get; set; }

    [Required]
    public int CreatedByUserId { get; set; }

    [Required]
    public DateTime? StartTime { get; set; } // make nullable to prevent weird data validation error

    [Required]
    public DateTime? EndTime { get; set; } // make nullable to prevent weird data validation error

    public bool UsesSmartboard { get; set; }

    // don't validate when posting forms
    [ValidateNever]
    public Room Room { get; set; }

    [ValidateNever]
    public User CreatedByUser { get; set; }

    [ValidateNever]
    public Group Group { get; set; }
}
