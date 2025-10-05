using Business.Models;
using Business.Models.Results;

namespace Business.Interfaces;
public interface IWorkoutLocationService
{
    Task<WorkoutLocationResult<IEnumerable<WorkoutLocationModel>>> GetAll();
}