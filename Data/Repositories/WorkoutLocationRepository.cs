using Data.Contexts;
using Data.Entities;
using Data.Interfaces;

namespace Data.Repositories;
public class WorkoutLocationRepository(DataContext context) : BaseRepository<WorkoutLocationEntity>(context), IWorkoutLocationRepository
{

}
