namespace DM2Projekt.Models
{
    public class Booking
    {
        public int BookingId { get; set; }
        
        public int GroupId { get; set; }
        public int RoomId { get; set; }
        public int CreateByUserId { get; set; }
        public int SmartboardId { get; set; }

        // nav properties
        public Room Room { get; set; }
        public User CreatedByUser { get; set; }
        public Group Group{ get;set; }
        public Smartboard Smartboard { get; set; }

    }
}
