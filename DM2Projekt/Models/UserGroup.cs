using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace DM2Projekt.Models;

public class UserGroup
{
    public int UserId { get; set; }
    public int GroupId { get; set; }

    [ValidateNever]
    public User User { get; set; }

    [ValidateNever]
    public Group Group { get; set; }
}
