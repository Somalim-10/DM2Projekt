using System.ComponentModel.DataAnnotations;

namespace DM2Projekt.Models;

public class GroupInvitation
{
    [Key]
    public int InvitationId { get; set; }

    public int GroupId { get; set; }
    public Group Group { get; set; } = null!;

    public int InvitedUserId { get; set; }
    public User InvitedUser { get; set; } = null!;

    public bool? IsAccepted { get; set; } // null = pending, true = accepteret, false = afvist

    public DateTime SentAt { get; set; } = DateTime.Now;
}
