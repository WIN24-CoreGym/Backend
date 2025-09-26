using Data.Contexts;
using Data.Entities;
using Data.Interfaces;

namespace Data.Repositories;

public class WorkoutRepository(DataContext context)
    : BaseRepository<WorkoutEntity>(context), IWorkoutRepository
{
    private readonly DataContext _context = context;

    public async Task AddInstructorRelationAsync(WorkoutInstructorEntity wi)
    {
        _context.WorkoutInstructor.Add(wi);
        await _context.SaveChangesAsync();
    }
}
