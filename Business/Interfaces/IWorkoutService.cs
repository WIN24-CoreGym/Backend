using Business.Models;
using Business.Models.Results;

namespace Business.Interfaces;
public interface IWorkoutService
{
    Task<WorkoutResult<IEnumerable<WorkoutModel>>> GetAllWorkoutsAsync();
    Task<WorkoutResult<IEnumerable<WorkoutModel>>> GetAvailableWorkoutsAsync();
}