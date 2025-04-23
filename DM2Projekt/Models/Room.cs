using DM2Projekt.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace DM2Projekt.Models;

public class Room
{
    [Key]
    public int RoomId { get; set; }

    [Required]
    public string RoomName { get; set; }

    public int Capacity { get; set; }
    public RoomType RoomType { get; set; }

    public bool CanBeShared { get; set; }

    public Smartboard? Smartboard { get; set; } // single smartboard
    public ICollection<Booking> Bookings { get; set; } = [];

}
