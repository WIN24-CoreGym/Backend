using Data.Contexts;
using Data.Entities;
using Data.Interfaces;

namespace Data.Repositories;
public class WorkoutRepository(DataContext context) : BaseRepository<WorkoutEntity>(context), IWorkoutRepository
{
}
