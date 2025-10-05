using Business.Interfaces;
using Business.Models;
using Business.Models.Results;
using Data.Interfaces;
using System.Threading.Tasks;

namespace Business.Services;
public class WorkoutLocationService(IWorkoutLocationRepository workoutLocationRepository) : IWorkoutLocationService
{
    private readonly IWorkoutLocationRepository _workoutLocationRepository = workoutLocationRepository;

    public async Task<WorkoutLocationResult<IEnumerable<WorkoutLocationModel>>> GetAll()
    {
        var result = await _workoutLocationRepository.GetAllAsync();
        if (!result.Succeeded || result.Data == null)
            return WorkoutLocationResult<IEnumerable<WorkoutLocationModel>>.InternalServerError("Failed retrieving workout locations.");

        var entities = result.Data;
        var locations = entities.Where(x => x != null).Select(x => new WorkoutLocationModel
        {
            Id = x.Id,
            Name = x.Name,
        }) ?? [];

        return WorkoutLocationResult<IEnumerable<WorkoutLocationModel>>.Ok(locations);
    }
}
