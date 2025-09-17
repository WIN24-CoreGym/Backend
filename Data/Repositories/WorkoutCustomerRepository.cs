using Data.Contexts;
using Data.Entities;
using Data.Interfaces;

namespace Data.Repositories;
public class WorkoutCustomerRepository(DataContext context) : BaseRepository<WorkoutCustomerEntity>(context), IWorkoutCustomerRepository
{

}
