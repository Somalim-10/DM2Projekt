using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace DM2Projekt.Models;

// Booking model
public class Booking
{
    [Key]
    public int BookingId { get; set; } // primary key

    [Required]
    public int GroupId { get; set; } // group who owns booking

    [Required]
    public int RoomId { get; set; } // room being booked

    [Required]
    public int CreatedByUserId { get; set; } // who created booking

    [ValidateNever]
    public DateTime? StartTime { get; set; } // when booking starts

    [ValidateNever]
    public DateTime? EndTime { get; set; } // when booking ends

    public bool UsesSmartboard { get; set; } // smartboard used?

    // navigation properties
    [ValidateNever]
    public Room Room { get; set; }

    [ValidateNever]
    public User CreatedByUser { get; set; }

    [ValidateNever]
    public Group Group { get; set; }
}
