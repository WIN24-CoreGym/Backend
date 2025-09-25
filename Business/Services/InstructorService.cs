using Business.Dtos.Instructor;
using Business.Interfaces;
using Business.Models.Results;
using Data.Entities;
using Data.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Business.Services;
public class InstructorService(IInstructorRepository instructorRepository) : IInstructorService
{
    private readonly IInstructorRepository _instructorRepository = instructorRepository;


    // CREATE
    public async Task<InstructorResult<InstructorDto>> CreateAsync(CreateInstructorDto dto)
    {
        if (dto == null)
            return InstructorResult<InstructorDto>.BadRequest("Input cannot be null.");

        var entity = new InstructorEntity
        {
            FirstName = dto.FirstName,
            LastName = dto.LastName
        };

        await _instructorRepository.AddAsync(entity);
        var result = await _instructorRepository.SaveAsync();
        if (!result.Succeeded)
            return InstructorResult<InstructorDto>.InternalServerError(result.ErrorMessage!);

        var createdEntity = (await _instructorRepository.GetOneByQueryAsync(query => query
            .Where(x => x.Id == entity.Id)
            .Include(x => x.WorkoutInstructors).ThenInclude(x => x.Workout))).Data;

        if (createdEntity == null)
            return InstructorResult<InstructorDto>.InternalServerError("Retrieved null entity after creation.");

        var instructor = new InstructorDto
        {
            Id = createdEntity.Id,
            FirstName = createdEntity.FirstName,
            LastName = createdEntity.LastName,
            Workouts = createdEntity.WorkoutInstructors?
                .Where(x => x.Workout != null)
                .Select(x => new InstructorWorkoutDto
                {
                    Id = x.Workout!.Id,
                    Title = x.Workout.Title,
                    Description = x.Workout.Description,
                    StartDate = x.Workout.StartDate,
                    EndDate = x.Workout.EndDate,
                    Location = x.Workout.Location != null ? x.Workout.Location.Name : string.Empty,
                    MaxBookings = x.Workout.MaxBookings,
                    Created = x.Workout.Created
                }).ToList() ?? []
        };

        return InstructorResult<InstructorDto>.Ok(instructor);
    }

    // READ
    public async Task<InstructorResult<IEnumerable<InstructorDto>>> GetAllAsync()
    {
        var result = await _instructorRepository.GetByQueryAsync(query => query
            .Include(x => x.WorkoutInstructors).ThenInclude(x => x.Workout));

        if (!result.Succeeded)
            return InstructorResult<IEnumerable<InstructorDto>>.InternalServerError("Failed retrieving entities.");

        var entities = result.Data;
        var instructors = entities?.Select(x => new InstructorDto
        {
            Id = x.Id,
            FirstName = x.FirstName,
            LastName = x.LastName,
            Workouts = x.WorkoutInstructors
                .Where(wi => wi.Workout != null)
                .Select(w => new InstructorWorkoutDto
                {
                    Id = w.Workout!.Id,
                    Title = w.Workout.Title,
                    Description = w.Workout.Description,
                    StartDate = w.Workout.StartDate,
                    EndDate = w.Workout.EndDate,
                    Location = w.Workout.Location != null ? w.Workout.Location.Name : string.Empty,
                    MaxBookings = w.Workout.MaxBookings,
                    Created = w.Workout.Created
                }).ToList() ?? []
        }) ?? [];

        return InstructorResult<IEnumerable<InstructorDto>>.Ok(instructors);
    }

    public async Task<InstructorResult<InstructorDto>> GetByIdAsync(int id)
    {
        var result = await _instructorRepository.GetOneByQueryAsync(query => query
            .Where(x => x.Id == id)
            .Include(x => x.WorkoutInstructors).ThenInclude(x => x.Workout));

        if (result.Data == null)
            return InstructorResult<InstructorDto>.NotFound($"Unable to find instructor with id {id}.");

        var e = result.Data;
        var instructor = new InstructorDto
        {
            Id = e.Id,
            FirstName = e.FirstName,
            LastName = e.LastName,
            Workouts = e.WorkoutInstructors?
                .Where(x => x.Workout != null)
                .Select(x => new InstructorWorkoutDto
                {
                    Id = x.Workout!.Id,
                    Title = x.Workout.Title,
                    Description = x.Workout.Description,
                    StartDate = x.Workout.StartDate,
                    EndDate = x.Workout.EndDate,
                    Location = x.Workout.Location != null ? x.Workout.Location.Name : string.Empty,
                    MaxBookings = x.Workout.MaxBookings,
                    Created = x.Workout.Created
                }).ToList() ?? []
        };

        return InstructorResult<InstructorDto>.Ok(instructor);
    }

    // UPDATE
    public async Task<InstructorResult<InstructorDto>> UpdateAsync(UpdateInstructorDto dto)
    {
        try
        {
            var result = await _instructorRepository.GetOneAsync(x => x.Id == dto.Id);
            var entity = result.Data;
            if (entity == null)
                return InstructorResult<InstructorDto>.NotFound($"Unable to find instructor with id {dto.Id}.");

            entity.FirstName = dto.FirstName;
            entity.LastName = dto.LastName;

            _instructorRepository.Update(entity);
            var updateResult = await _instructorRepository.SaveAsync();
            if (!updateResult.Succeeded)
                return InstructorResult<InstructorDto>.InternalServerError($"Failed updating instructor with id {dto.Id}.");

            var updatedEntity = (await _instructorRepository.GetOneByQueryAsync(query => query
            .Where(x => x.Id == dto.Id)
            .Include(x => x.WorkoutInstructors).ThenInclude(x => x.Workout))).Data;

            var instructor = new InstructorDto
            {
                Id = updatedEntity!.Id,
                FirstName = updatedEntity.FirstName,
                LastName = updatedEntity.LastName,
                Workouts = updatedEntity.WorkoutInstructors?
                .Where(x => x.Workout != null)
                .Select(x => new InstructorWorkoutDto
                {
                    Id = x.Workout!.Id,
                    Title = x.Workout.Title,
                    Description = x.Workout.Description,
                    StartDate = x.Workout.StartDate,
                    EndDate = x.Workout.EndDate,
                    Location = x.Workout.Location != null ? x.Workout.Location.Name : string.Empty,
                    MaxBookings = x.Workout.MaxBookings,
                    Created = x.Workout.Created
                }).ToList() ?? []
            };

            return InstructorResult<InstructorDto>.Ok(instructor);
        }
        catch (Exception ex)
        {
            return InstructorResult<InstructorDto>.InternalServerError("Unexpected error occured.");
        }
    }

    // DELETE
    public async Task<InstructorResult<InstructorDto>> DeleteByIdAsync(int id)
    {
        var entity = (await _instructorRepository.GetOneAsync(x => x.Id == id)).Data;
        if (entity == null)
            return InstructorResult<InstructorDto>.NotFound($"Unable to find instructor with id {id}.");

        _instructorRepository.Delete(entity);
        var result = await _instructorRepository.SaveAsync();
        return result.Succeeded
            ? InstructorResult<InstructorDto>.Ok()
            : InstructorResult<InstructorDto>.InternalServerError($"Failed deleting instructor with id {id}.");
    }
}
