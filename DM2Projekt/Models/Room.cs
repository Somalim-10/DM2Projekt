using System.ComponentModel.DataAnnotations;

namespace DM2Projekt.Models;

public class Room
{
    [Key]
    public int RoomId { get; set; }

    [Required]
    public string RoomName { get; set; }

    public RoomType RoomType { get; set; }

    // optional. for UI display
    [Url]
    [RegularExpression(@".*\.(jpg|jpeg|png|gif|bmp|webp)$", ErrorMessage = "URL skal pege på et billede (.jpg, .jpeg, .png, .gif, .bmp, .webp)")]
    public string? ImageUrl { get; set; }

    [Required(ErrorMessage = "Bygning skal vælges.")]
    public Building Building { get; set; }

    [Required(ErrorMessage = "Etage skal vælges.")]
    public Floor Floor { get; set; }

    // bookings for this room
    public ICollection<Booking> Bookings { get; set; } = [];
}
