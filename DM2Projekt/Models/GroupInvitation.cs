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

    // null = pending, true = accepted, false = rejected
    public bool? IsAccepted { get; set; }

    // default = now
    public DateTime SentAt { get; set; } = DateTime.Now;
}
