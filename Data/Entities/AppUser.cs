using Microsoft.AspNetCore.Identity;

namespace Data.Entities;

public class AppUser : IdentityUser
{
    public ICollection<WorkoutCustomerEntity> WorkoutIds { get; set; } = [];
}
