using Data.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Data.Contexts;

public class DataContext(DbContextOptions<DataContext> options) : IdentityDbContext<AppUser>(options)
{
    public DbSet<WorkoutEntity> Workout { get; set; }
    public DbSet<WorkoutCustomerEntity> WorkoutCustomer { get; set; }
    public DbSet<WorkoutInstructorEntity> WorkoutInstructor { get; set; }
    public DbSet<WorkoutLocationEntity> WorkoutLocation { get; set; }
    public DbSet<InstructorEntity> Instructor { get; set; }
}
