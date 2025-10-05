using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Business.Factories;
using Business.Interfaces;
using Business.Models;
using Business.Models.Results;
using Business.Dtos.Workouts;
using Business.Models.Mappers;
using Data.Contexts;
using Data.Entities;
using Data.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Business.Services;

public class WorkoutService(DataContext ctx, IWorkoutRepository workoutRepository, UserManager<AppUser> userManager) : IWorkoutService
{
    private readonly DataContext _ctx = ctx;
    private readonly IWorkoutRepository _workoutRepository = workoutRepository;
    private readonly UserManager<AppUser> _userManager = userManager;

    public async Task<WorkoutResult<IEnumerable<WorkoutModel>>> GetAllWorkoutsAsync()
    {
        var repositoryResult = await _workoutRepository.GetByQueryAsync(query => query
            .Include(x => x.Location)
            .Include(x => x.Instructors).ThenInclude(x => x.Instructor)
            .Include(x => x.Customers));

        if (!repositoryResult.Succeeded)
            return WorkoutResult<IEnumerable<WorkoutModel>>.InternalServerError("Failed retrieving workout entities.");

        var entities = repositoryResult.Data ?? [];
        var workouts = entities.Select(WorkoutFactory.ToModel);

        return WorkoutResult<IEnumerable<WorkoutModel>>.Ok(workouts);
    }

    public async Task<WorkoutResult<IEnumerable<WorkoutModel>>> GetAvailableWorkoutsAsync()
    {
        var repositoryResult = await _workoutRepository.GetByQueryAsync(query => query
            .Where(x => x.Customers.Count() < x.MaxBookings && x.StartDate > DateTime.Now)
            .Include(x => x.Location)
            .Include(x => x.Instructors).ThenInclude(x => x.Instructor)
            .Include(x => x.Customers));

        if (!repositoryResult.Succeeded)
            return WorkoutResult<IEnumerable<WorkoutModel>>.InternalServerError("Failed retrieving workout entities.");

        var entities = repositoryResult.Data ?? [];
        var workouts = entities.Select(WorkoutFactory.ToModel);

        return WorkoutResult<IEnumerable<WorkoutModel>>.Ok(workouts);
    }

    public async Task<WorkoutResult<IEnumerable<WorkoutModel>>> GetWorkoutsByCustomerIdAsync(string userId)
    {
        var repositoryResult = await _workoutRepository.GetByQueryAsync(query => query
            .Where(x => x.Customers.Any(c => c.UserId == userId))
            .Include(x => x.Location)
            .Include(x => x.Instructors).ThenInclude(x => x.Instructor));

        if (!repositoryResult.Succeeded)
            return WorkoutResult<IEnumerable<WorkoutModel>>.InternalServerError("Failed retrieving workout entities.");

        var entities = repositoryResult.Data ?? [];
        var workouts = entities.Select(WorkoutFactory.ToModel);

        return WorkoutResult<IEnumerable<WorkoutModel>>.Ok(workouts);
    }

    public async Task<WorkoutResult<WorkoutModel>> GetWorkoutByIdAsync(int id)
    {
        var repositoryResult = await _workoutRepository.GetByQueryAsync(query => query
            .Where(x => x.Id == id)
            .Include(x => x.Location)
            .Include(x => x.Instructors).ThenInclude(x => x.Instructor)
            .Include(x => x.Customers));

        if (!repositoryResult.Succeeded)
            return WorkoutResult<WorkoutModel>.InternalServerError("Failed retrieving workout entity.");

        var entity = repositoryResult.Data?.FirstOrDefault();
        if (entity is null)
            return WorkoutResult<WorkoutModel>.NotFound("Workout not found.");

        return WorkoutResult<WorkoutModel>.Ok(WorkoutFactory.ToModel(entity));
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

    // CREATE - från main branch
    public async Task<WorkoutResponse> CreateAsync(CreateWorkoutRequest req, CancellationToken ct = default)
    {
        if (req.StartDate < DateTime.UtcNow)
            throw new InvalidOperationException("Start date must be today or later.");

        var locationExists = await _ctx.WorkoutLocation.AnyAsync(l => l.Id == req.LocationId, ct);
        if (!locationExists)
            throw new KeyNotFoundException($"Location with id {req.LocationId} was not found.");

        var instructorExists = await _ctx.Instructor.AnyAsync(i => i.Id == req.InstructorId, ct);
        if (!instructorExists)
            throw new KeyNotFoundException($"Instructor with id {req.InstructorId} was not found.");

        var clash = await _ctx.Workout.AnyAsync(w =>
            w.StartDate == req.StartDate &&
            w.LocationId == req.LocationId, ct);
        if (clash)
            throw new InvalidOperationException("This location is already booked at the selected time.");

        var entity = req.ToEntity();

        if (entity.MaxBookings <= 0)
            entity.MaxBookings = 20;
        if (entity.EndDate == default)
            entity.EndDate = entity.StartDate;

        _ctx.Workout.Add(entity);
        await _ctx.SaveChangesAsync(ct);

        var wi = new WorkoutInstructorEntity
        {
            WorkoutId = entity.Id,
            InstructorId = req.InstructorId
        };
        _ctx.WorkoutInstructor.Add(wi);
        await _ctx.SaveChangesAsync(ct);

        await _ctx.Entry(entity).Reference(e => e.Location).LoadAsync(ct);
        await _ctx.Entry(entity).Collection(e => e.Instructors)
                 .Query().Include(i => i.Instructor)
                 .LoadAsync(ct);

        return entity.ToResponse();
    }
}
