using System.ComponentModel.DataAnnotations;

namespace DM2Projekt.Models
{
    public class Room
    {
   
        //classNameId
        //ID
        // "d" I Id skal være med et lille "d", da der kan opstå konflikter i projektet. Så ved den også at det en PrimaryKey.
        public int RoomId { get; set; } 
        public string RoomName { get; set; }
        public int Capacity { get; set; } // Number of people
        public string RoomType { get; set; }
        public bool CanBeShared{ get; set; }

    }
}
