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

public class WorkoutService(DataContext ctx, IWorkoutRepository workoutRepository, IWorkoutLocationRepository workoutLocationRepository, IInstructorRepository instructorRepository, IWorkoutInstructorRepository workoutInstructorRepository, UserManager<AppUser> userManager) : IWorkoutService
{
    private readonly DataContext _ctx = ctx;
    private readonly IWorkoutRepository _workoutRepository = workoutRepository;
    private readonly IWorkoutLocationRepository _workoutLocationRepository = workoutLocationRepository;
    private readonly IWorkoutInstructorRepository _workoutInstructorRepository = workoutInstructorRepository;
    private readonly IInstructorRepository _instructorRepository = instructorRepository;
    private readonly UserManager<AppUser> _userManager = userManager;

    public async Task<WorkoutResult<IEnumerable<WorkoutModel>>> GetAllWorkoutsAsync()
    {
        var repositoryResult = await _workoutRepository.GetByQueryAsync(query => query
            .Include(x => x.Location)
            .Include(x => x.Instructors).ThenInclude(x => x.Instructor)
            .Include(x => x.Customers).ThenInclude(x => x.Customer));

        if (!repositoryResult.Succeeded)
            return WorkoutResult<IEnumerable<WorkoutModel>>.InternalServerError("Failed retrieving workout entities.");

        var entities = repositoryResult.Data ?? [];
        var workouts = entities.Select(x => x.ToModel());

        return WorkoutResult<IEnumerable<WorkoutModel>>.Ok(workouts);
    }

    public async Task<WorkoutResult<IEnumerable<WorkoutModel>>> GetAvailableWorkoutsAsync()
    {
        var repositoryResult = await _workoutRepository.GetByQueryAsync(query => query
            .Where(x => x.Customers.Count() < x.MaxBookings && x.StartDate > DateTime.Now)
            .Include(x => x.Location)
            .Include(x => x.Instructors).ThenInclude(x => x.Instructor)
            .Include(x => x.Customers).ThenInclude(x => x.Customer));

        if (!repositoryResult.Succeeded)
            return WorkoutResult<IEnumerable<WorkoutModel>>.InternalServerError("Failed retrieving workout entities.");

        var entities = repositoryResult.Data ?? [];
        var workouts = entities.Select(x => x.ToModel());

        return WorkoutResult<IEnumerable<WorkoutModel>>.Ok(workouts);
    }

    public async Task<WorkoutResult<IEnumerable<WorkoutModel>>> GetWorkoutsByCustomerIdAsync(string userId)
    {
        var repositoryResult = await _workoutRepository.GetByQueryAsync(query => query
            .Where(x => x.Customers.Any(c => c.UserId == userId))
            .Include(x => x.Location)
            .Include(x => x.Instructors).ThenInclude(x => x.Instructor)
            .Include(x => x.Customers).ThenInclude(x => x.Customer));

        if (!repositoryResult.Succeeded)
            return WorkoutResult<IEnumerable<WorkoutModel>>.InternalServerError("Failed retrieving workout entities.");

        var entities = repositoryResult.Data ?? [];
        var workouts = entities.Select(x => x.ToModel());

        return WorkoutResult<IEnumerable<WorkoutModel>>.Ok(workouts);
    }

    public async Task<WorkoutResult<WorkoutModel>> GetWorkoutByIdAsync(int id)
    {
        var repositoryResult = await _workoutRepository.GetByQueryAsync(query => query
            .Where(x => x.Id == id)
            .Include(x => x.Location)
            .Include(x => x.Instructors).ThenInclude(x => x.Instructor)
            .Include(x => x.Customers).ThenInclude(x => x.Customer));

        if (!repositoryResult.Succeeded)
            return WorkoutResult<WorkoutModel>.InternalServerError("Failed retrieving workout entity.");

        var entity = repositoryResult.Data?.FirstOrDefault();
        if (entity is null)
            return WorkoutResult<WorkoutModel>.NotFound("Workout not found.");

        var workout = entity.ToModel();

        return WorkoutResult<WorkoutModel>.Ok(workout);
    }

    // UPDATE
    public async Task<WorkoutResult<WorkoutModel>> UpdateWorkoutAsync(int workoutId, UpdateWorkoutRequest req)
    {
        try
        {
            await _workoutRepository.BeginTransactionAsync();

            // Kontrollera att träningspasset finns
            var existingResult = await _workoutRepository.GetOneByQueryAsync(q => q
                .Where(x => x.Id == workoutId)
                .Include(x => x.Instructors));
            if (!existingResult.Succeeded || existingResult.Data == null)
                return WorkoutResult<WorkoutModel>.NotFound("Workout not found.");

            // Konvertera till entity och sätt rätt ID
            var entity = existingResult.Data;
            entity.Title = req.Title;
            entity.Description = req.Description;
            entity.StartDate = req.StartDate;
            entity.EndDate = req.EndDate;
            entity.MaxBookings = req.MaxBookings;

            // Check if location already exists, if not then create it
            var locationExists = await _ctx.WorkoutLocation.AnyAsync(l => l.Name == req.Location);
            if (!locationExists)
            {
                await _workoutLocationRepository.AddAsync(new WorkoutLocationEntity { Name = req.Location });
                var result = await _workoutLocationRepository.SaveAsync();
                if (!result.Succeeded)
                {
                    await _workoutRepository.RollbackTransactionAsync();
                    return WorkoutResult<WorkoutModel>.InternalServerError("Failed creating missing location. Rolling back.");
                }
            }

            var locationResult = await _workoutLocationRepository.GetOneAsync(x => x.Name == req.Location);
            if (!locationResult.Succeeded || locationResult.Data == null)
            {
                await _workoutRepository.RollbackTransactionAsync();
                return WorkoutResult<WorkoutModel>.InternalServerError("Failed retrieving location after creation. Rolling back.");
            }

            var locationId = locationResult.Data.Id;
            entity.LocationId = locationId;

            // Uppdatera entiteten
            var updateResult = _workoutRepository.Update(entity);
            if (!updateResult.Succeeded)
            {
                await _workoutRepository.RollbackTransactionAsync();
                return WorkoutResult<WorkoutModel>.InternalServerError("Failed to update workout entity.");
            }

            // Spara ändringar
            var saveResult = await _workoutRepository.SaveAsync();
            if (!saveResult.Succeeded)
            {
                await _workoutRepository.RollbackTransactionAsync();
                return WorkoutResult<WorkoutModel>.InternalServerError("Failed to save workout changes.");
            }

            // Update instructors
            var currentInstructorIds = entity.Instructors.Select(x => x.InstructorId).ToList();
            var instructorIdsToRemove = currentInstructorIds.Except(req.InstructorIds).ToList();
            var instructorIdsToAdd = req.InstructorIds.Except(currentInstructorIds).ToList();

            // Delete WorkoutInstructor
            foreach (var id in instructorIdsToRemove)
            {
                var entityResult = await _workoutInstructorRepository.GetOneByQueryAsync(q => q
                    .Where(x => x.WorkoutId == workoutId && x.InstructorId == id));
                if (!entityResult.Succeeded ||  entityResult.Data == null)
                {
                    await _workoutRepository.RollbackTransactionAsync();
                    return WorkoutResult<WorkoutModel>.InternalServerError("Failed retrieving existing instructor entity for deletion. Rolling back.");
                }

                _workoutInstructorRepository.Delete(entityResult.Data);
                var deletionResult = await _workoutInstructorRepository.SaveAsync();
                if (!deletionResult.Succeeded)
                {
                    await _workoutRepository.RollbackTransactionAsync();
                    return WorkoutResult<WorkoutModel>.InternalServerError("Failed deleting existing instructor. Rolling back.");
                }
            }

            // Add WorkoutInstructor
            foreach (var id in instructorIdsToAdd)
            {
                var workoutInstructorEntity = new WorkoutInstructorEntity
                {
                    WorkoutId = workoutId,
                    InstructorId = id
                };

                await _workoutInstructorRepository.AddAsync(workoutInstructorEntity);
                var addResult = await _workoutInstructorRepository.SaveAsync();
                if (!addResult.Succeeded)
                {
                    await _workoutRepository.RollbackTransactionAsync();
                    return WorkoutResult<WorkoutModel>.InternalServerError("Failed adding new instructor. Rolling back.");
                }
            }

            await _workoutRepository.CommitTransactionAsync();

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

    // CREATE
    public async Task<WorkoutResult<WorkoutModel>> CreateAsync(CreateWorkoutRequest req, CancellationToken ct = default)
    {
        try
        {
            await _workoutRepository.BeginTransactionAsync();

            if (req.StartDate < DateTime.UtcNow)
                throw new InvalidOperationException("Start date must be today or later.");

            // Check if location already exists, if not then create it
            var locationExists = await _ctx.WorkoutLocation.AnyAsync(l => l.Name == req.Location, ct);
            if (!locationExists)
            {
                await _workoutLocationRepository.AddAsync(new WorkoutLocationEntity { Name = req.Location });
                var result = await _workoutLocationRepository.SaveAsync();
                if (!result.Succeeded)
                {
                    await _workoutRepository.RollbackTransactionAsync();
                    return WorkoutResult<WorkoutModel>.InternalServerError("Failed creating missing location. Rolling back.");
                }
            }

            var locationResult = await _workoutLocationRepository.GetOneAsync(x => x.Name == req.Location);
            if (!locationResult.Succeeded || locationResult.Data == null)
            {
                await _workoutRepository.RollbackTransactionAsync();
                return WorkoutResult<WorkoutModel>.InternalServerError("Failed retrieving location after creation. Rolling back.");
            }
            var locationId = locationResult.Data.Id;

            // Loop through list of instructorIds and verify that they exist
            foreach (var InstructorId in req.InstructorIds)
            {
                var existsResult = await _instructorRepository.ExistsAsync(x => x.Id == InstructorId);
                if (existsResult.Succeeded && existsResult.StatusCode == 404)
                {
                    await _workoutRepository.RollbackTransactionAsync();
                    return WorkoutResult<WorkoutModel>.InternalServerError($"Instructor with id {InstructorId} not found. Rolling back.");
                }
            }

            var clash = await _ctx.Workout.AnyAsync(w =>
                w.StartDate == req.StartDate &&
                w.LocationId == locationId, ct);
            if (clash)
                throw new InvalidOperationException("This location is already booked at the selected time.");

            var entity = req.ToEntity();
            entity.LocationId = locationId;

            if (entity.MaxBookings <= 0)
                entity.MaxBookings = 20;
            if (entity.EndDate == default)
                entity.EndDate = entity.StartDate;

            _ctx.Workout.Add(entity);
            await _ctx.SaveChangesAsync(ct);

            foreach (var instructorId in req.InstructorIds)
            {
                var wi = new WorkoutInstructorEntity
                {
                    WorkoutId = entity.Id,
                    InstructorId = instructorId,
                };
                _ctx.WorkoutInstructor.Add(wi);
            }
            await _ctx.SaveChangesAsync(ct);

            await _ctx.Entry(entity).Reference(e => e.Location).LoadAsync(ct);
            await _ctx.Entry(entity).Collection(e => e.Instructors)
                     .Query().Include(i => i.Instructor)
                     .LoadAsync(ct);

            var getWorkoutResult = await _workoutRepository.GetOneByQueryAsync(q => q
                .Where(x => x.Id == entity.Id)
                .Include(x => x.Instructors).ThenInclude(x => x.Instructor)
                .Include(x => x.Customers).ThenInclude(x => x.Customer));

            if (!getWorkoutResult.Succeeded || getWorkoutResult.Data == null)
            {
                await _workoutRepository.RollbackTransactionAsync();
                return WorkoutResult<WorkoutModel>.InternalServerError("Failed retrieving workout after creation. Rolling back.");
            }

            var workout = getWorkoutResult.Data.ToModel();
            await _workoutRepository.CommitTransactionAsync();

            return WorkoutResult<WorkoutModel>.Created(workout);
        }
        catch
        {
            await _workoutRepository.RollbackTransactionAsync();
            return WorkoutResult<WorkoutModel>.InternalServerError($"Unhandled error occured. Rolling back.");
        }
    }
}
