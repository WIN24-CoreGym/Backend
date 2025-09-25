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

    // Create workout - Admin only

    // CREATE
    public async Task<WorkoutResult<WorkoutModel>> CreateWorkoutAsync(WorkoutModel workoutModel)
    {
        try
        {
            // Konvertera WorkoutModel till WorkoutEntity via factory
            var entity = WorkoutFactory.ToEntity(workoutModel);

            // Lägg till entiteten i databasen
            var addResult = await _workoutRepository.AddAsync(entity);
            if (!addResult.Succeeded)
                return WorkoutResult<WorkoutModel>.InternalServerError("Failed to add workout entity.");

            // Spara ändringar
            var saveResult = await _workoutRepository.SaveAsync();
            if (!saveResult.Succeeded)
                return WorkoutResult<WorkoutModel>.InternalServerError("Failed to save workout.");

            // Hämta det skapade träningspasset med alla inkluderade data
            var createdWorkout = await GetWorkoutByIdAsync(entity.Id);
            return createdWorkout;
        }
        catch (Exception)
        {
            return WorkoutResult<WorkoutModel>.InternalServerError("Exception occurred while creating workout.");
        }
    }

    // UPDATE
    public async Task<WorkoutResult<WorkoutModel>> UpdateWorkoutAsync(int workoutId, WorkoutModel workoutModel)
    {
        try
        {
            // Kontrollera att träningspasset finns
            var existingResult = await _workoutRepository.GetOneAsync(x => x.Id == workoutId);
            if (!existingResult.Succeeded)
                return WorkoutResult<WorkoutModel>.NotFound("Workout not found.");

            // Konvertera till entity och sätt rätt ID
            var entity = WorkoutFactory.ToEntity(workoutModel);
            entity.Id = workoutId;

            // Uppdatera entiteten
            var updateResult = _workoutRepository.Update(entity);
            if (!updateResult.Succeeded)
                return WorkoutResult<WorkoutModel>.InternalServerError("Failed to update workout entity.");

            // Spara ändringar
            var saveResult = await _workoutRepository.SaveAsync();
            if (!saveResult.Succeeded)
                return WorkoutResult<WorkoutModel>.InternalServerError("Failed to save workout changes.");

            // Returnera det uppdaterade träningspasset
            var updatedWorkout = await GetWorkoutByIdAsync(workoutId);
            return updatedWorkout;
        }
        catch (Exception)
        {
            return WorkoutResult<WorkoutModel>.InternalServerError("Exception occurred while updating workout.");
        }
    }

    // DELETE
    public async Task<WorkoutResult<bool?>> DeleteWorkoutAsync(int workoutId)
    {
        try
        {
            // Kontrollera att träningspasset finns
            var existingResult = await _workoutRepository.GetOneAsync(x => x.Id == workoutId);
            if (!existingResult.Succeeded)
                return WorkoutResult<bool?>.NotFound("Workout not found.");

            // Ta bort entiteten
            var deleteResult = _workoutRepository.Delete(existingResult.Data!);
            if (!deleteResult.Succeeded)
                return WorkoutResult<bool?>.InternalServerError("Failed to delete workout entity.");

            // Spara ändringar
            var saveResult = await _workoutRepository.SaveAsync();
            if (!saveResult.Succeeded)
                return WorkoutResult<bool?>.InternalServerError("Failed to save workout deletion.");

            return WorkoutResult<bool?>.Ok(true);
        }
        catch (Exception)
        {
            return WorkoutResult<bool?>.InternalServerError("Exception occurred while deleting workout.");
        }
    }

    // READ BY ID
    public async Task<WorkoutResult<WorkoutModel>> GetWorkoutByIdAsync(int workoutId)
    {
        try
        {
            var repositoryResult = await _workoutRepository.GetOneAsync(
                x => x.Id == workoutId,
                x => x.Location,
                x => x.Instructors,
                x => x.Customers);

            if (!repositoryResult.Succeeded)
                return WorkoutResult<WorkoutModel>.NotFound("Workout not found.");

            var workout = WorkoutFactory.ToModel(repositoryResult.Data!);
            return WorkoutResult<WorkoutModel>.Ok(workout);
        }
        catch (Exception)
        {
            return WorkoutResult<WorkoutModel>.InternalServerError("Exception occurred while retrieving workout.");
        }
    }
}
