using System.ComponentModel.DataAnnotations;

namespace DM2Projekt.Models;

public class Group
{
    [Key]
    public int GroupId { get; set; }

    [Required]
    public string GroupName { get; set; }

    public ICollection<User> Users { get; set; } = [];
}
