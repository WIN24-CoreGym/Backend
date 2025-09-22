using Business.Factories;
using Business.Interfaces;
using Business.Models;
using Business.Models.Results;
using Data.Entities;
using Data.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Business.Services;
public class WorkoutService(IWorkoutRepository workoutRepository, UserManager<AppUser> userManager) : IWorkoutService
{
    private readonly IWorkoutRepository _workoutRepository = workoutRepository;
    private readonly UserManager<AppUser> _userManager = userManager;


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

    public async Task<WorkoutResult<IEnumerable<WorkoutModel>>> GetWorkoutsByCustomerIdAsync(string userId)
    {
        var repositoryResult = await _workoutRepository.GetByQueryAsync(query => query
            .Where(x => x.Customers.Any(x => x.UserId == userId))
            .Include(x => x.Location)
            .Include(x => x.Instructors)
            .ThenInclude(x => x.Instructor));

        if (!repositoryResult.Succeeded)
            return WorkoutResult<IEnumerable<WorkoutModel>>.InternalServerError("Failed retrieving workout entities.");

        var entities = repositoryResult.Data ?? [];
        var workouts = entities?.Select(x => WorkoutFactory.ToModel(x));

        return WorkoutResult<IEnumerable<WorkoutModel>>.Ok(workouts!);
    }
}
