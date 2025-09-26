using Business.Models;
using Business.Models.Results;
using Business.Dtos.Workouts;
using System.Threading;
using System.Threading.Tasks;

namespace Business.Interfaces;

public interface IWorkoutService
{
    Task<WorkoutResult<IEnumerable<WorkoutModel>>> GetAllWorkoutsAsync();
    Task<WorkoutResult<IEnumerable<WorkoutModel>>> GetAvailableWorkoutsAsync();
    Task<WorkoutResult<IEnumerable<WorkoutModel>>> GetWorkoutsByCustomerIdAsync(string userId);
    Task<WorkoutResult<WorkoutModel>> GetWorkoutByIdAsync(int workoutId);
    Task<WorkoutResult<WorkoutModel>> UpdateWorkoutAsync(int workoutId, WorkoutModel workoutModel);
    Task<WorkoutResult<bool?>> DeleteWorkoutAsync(int workoutId);
    Task<WorkoutResponse> CreateAsync(CreateWorkoutRequest req, CancellationToken ct = default);
}