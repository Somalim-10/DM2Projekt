using System.ComponentModel.DataAnnotations;

namespace DM2Projekt.Models;

public class Room
{

    public int RoomId { get; set; }
    public string RoomName { get; set; }
    public int Capacity { get; set; } // Number of people
    public string RoomType { get; set; }
    public bool CanBeShared { get; set; }

}
