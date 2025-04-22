using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DM2Projekt.Models;

public class Smartboard
{
    [Key]
    public int SmartboardId { get; set; }

    [ForeignKey("Room")]
    public int RoomId { get; set; }

    public bool IsAvailable { get; set; }

    public Room Room { get; set; }
}
