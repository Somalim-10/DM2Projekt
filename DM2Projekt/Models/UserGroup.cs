using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace DM2Projekt.Models;

// this links users and groups (many-to-many)
public class UserGroup
{
    public int UserId { get; set; }
    public int GroupId { get; set; }

    // don't validate when posting forms
    [ValidateNever]
    public User User { get; set; }

    [ValidateNever]
    public Group Group { get; set; }
}
