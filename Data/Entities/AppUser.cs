using Microsoft.AspNetCore.Identity;

namespace Data.Entities;

public class AppUser : IdentityUser
{
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public DateTime DateJoined { get; set; } = DateTime.UtcNow;

}
