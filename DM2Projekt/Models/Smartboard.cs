namespace DM2Projekt.Models
{
    public class Smartboard
    {
        public int SmartboardId { get; set; }
        public int RoomId { get; set; }
        public bool IsAvailable { get; set; }


        public Room Room { get; set; }
    }
}
