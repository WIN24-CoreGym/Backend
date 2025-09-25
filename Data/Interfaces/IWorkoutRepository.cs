using Data.Entities;

namespace Data.Interfaces;

public interface IWorkoutRepository : IBaseRepository<WorkoutEntity>
{
    Task AddInstructorRelationAsync(WorkoutInstructorEntity wi);
}
