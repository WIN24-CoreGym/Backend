using Business.Factories;
using Business.Interfaces;
using Business.Models;
using Business.Models.Results;
using Data.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Business.Services;
public class WorkoutService(IWorkoutRepository workoutRepository) : IWorkoutService
{
    private readonly IWorkoutRepository _workoutRepository = workoutRepository;


    // READ
    public async Task<WorkoutResult<IEnumerable<WorkoutModel>>> GetAllWorkoutsAsync()
    {
        var repositoryResult = await _workoutRepository.GetByQueryAsync(query => query
            .Include(x => x.Location)
            .Include(x => x.Instructors)
            .ThenInclude(x => x.Instructor)
            .Include(x => x.Customers));

        if (!repositoryResult.Succeeded)
            return WorkoutResult<IEnumerable<WorkoutModel>>.InternalServerError("Failed retrieving workout entities.");

        var entities = repositoryResult.Data ?? [];
        var workouts = entities?.Select(x => WorkoutFactory.ToModel(x));

        return WorkoutResult<IEnumerable<WorkoutModel>>.Ok(workouts!);
    }

    public async Task<WorkoutResult<IEnumerable<WorkoutModel>>> GetAvailableWorkoutsAsync()
    {
        var repositoryResult = await _workoutRepository.GetByQueryAsync(query => query
            .Where(x => x.Customers.Count() < x.MaxBookings && x.StartDate > DateTime.Now)
            .Include(x => x.Location)
            .Include(x => x.Instructors)
            .ThenInclude(x => x.Instructor)
            .Include(x => x.Customers));

        if (!repositoryResult.Succeeded)
            return WorkoutResult<IEnumerable<WorkoutModel>>.InternalServerError("Failed retrieving workout entities.");

        var entities = repositoryResult.Data ?? [];
        var workouts = entities?.Select(x => WorkoutFactory.ToModel(x));

        return WorkoutResult<IEnumerable<WorkoutModel>>.Ok(workouts!);
    }
}
