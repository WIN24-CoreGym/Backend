using Data.Contexts;
using Data.Entities;
using Data.Interfaces;

namespace Data.Repositories;
public class WorkoutInstructorRepository(DataContext context) : BaseRepository<WorkoutInstructorEntity>(context), IWorkoutInstructorRepository
{

}
