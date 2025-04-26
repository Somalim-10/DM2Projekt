using DM2Projekt.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace DM2Projekt.Models;

// room model
public class Room
{
    [Key]
    public int RoomId { get; set; }

    [Required]
    public string RoomName { get; set; }

    public RoomType RoomType { get; set; } // classroom or meeting room

    // bookings for this room
    public ICollection<Booking> Bookings { get; set; } = [];
}
