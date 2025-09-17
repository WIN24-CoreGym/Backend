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


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Workout
        modelBuilder.Entity<WorkoutEntity>()
            .HasMany(w => w.Customers)
            .WithOne(wc => wc.Workout);

        modelBuilder.Entity<WorkoutEntity>()
            .HasOne(w => w.Location)
            .WithMany(l => l.Workouts);

        modelBuilder.Entity<WorkoutEntity>()
            .HasMany(w => w.Instructors)
            .WithOne(i => i.Workout);


        // Instructor
        modelBuilder.Entity<InstructorEntity>()
            .HasMany(i => i.WorkoutInstructors)
            .WithOne(wi => wi.Instructor);


        // WorkoutInstructor
        modelBuilder.Entity<WorkoutInstructorEntity>()
            .HasKey(wi => new { wi.WorkoutId, wi.InstructorId });


        // WorkoutCustomer
        modelBuilder.Entity<WorkoutCustomerEntity>()
            .HasKey(wc => new { wc.WorkoutId, wc.UserId });

        base.OnModelCreating(modelBuilder);
    }
}
