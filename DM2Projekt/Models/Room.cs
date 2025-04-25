using DM2Projekt.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace DM2Projekt.Models;

public class Room
{
    [Key]
    public int RoomId { get; set; }

    [Required]
    public string RoomName { get; set; }

    public RoomType RoomType { get; set; }

    public ICollection<Booking> Bookings { get; set; } = [];
}
