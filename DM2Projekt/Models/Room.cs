using System.ComponentModel.DataAnnotations;

namespace DM2Projekt.Models
{
    public class Room
    {
        [Key]
        public int RoomId { get; set; }

        [Required]
        public string RoomName { get; set; }

        public int Capacity { get; set; }

        public string RoomType { get; set; }

        public bool CanBeShared { get; set; }

        // Navigation Properties
        public ICollection<Smartboard> Smartboards { get; set; } = new List<Smartboard>();
    }
}
