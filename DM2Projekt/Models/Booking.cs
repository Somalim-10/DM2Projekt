using DM2Projekt.Models.Enums;
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

    public int? SmartboardId { get; set; }

    [Required]
    public DateTime StartTime { get; set; }

    [Required]
    public DateTime EndTime { get; set; }

    [Required]
    public BookingStatus Status { get; set; }

    // Navigation properties
    public Room Room { get; set; }
    public User CreatedByUser { get; set; }
    public Group Group { get; set; }
    public Smartboard? Smartboard { get; set; }

}
