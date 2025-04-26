using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace DM2Projekt.Models;

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

    [ValidateNever]
    public DateTime? StartTime { get; set; }

    [ValidateNever]
    public DateTime? EndTime { get; set; }

    public bool UsesSmartboard { get; set; }

    [ValidateNever]
    public Room Room { get; set; }

    [ValidateNever]
    public User CreatedByUser { get; set; }

    [ValidateNever]
    public Group Group { get; set; }
}
