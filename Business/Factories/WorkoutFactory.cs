using Business.Dtos.API;
using Business.Models;
using Data.Entities;
using Microsoft.Identity.Client;
using WebApi.Dtos;

namespace Business.Factories;

public static class WorkoutFactory
{
    public static WorkoutModel ToModel(WorkoutEntity entity)
    {
        return new WorkoutModel
        {
            Id = entity.Id,
            Title = entity.Title,
            Description = entity.Description,
            StartDate = entity.StartDate,
            EndDate = entity.EndDate,
            LocationId = entity.LocationId,
            MaxBookings = entity.MaxBookings,
            Created = entity.Created,
            Location = entity.Location == null ? null : new WorkoutLocationModel
            {
                Id = entity.Location.Id,
                Name = entity.Location.Name,
            },
            WorkoutInstructors = entity.Instructors?.Select(i => new WorkoutInstructorModel
            {
                WorkoutId = i.WorkoutId,
                InstructorId = i.InstructorId,
                Instructor = i.Instructor == null ? null : new InstructorModel
                {
                    Id = i.Instructor.Id,
                    FirstName = i.Instructor.FirstName,
                    LastName = i.Instructor.LastName,
                }
            }),
            WorkoutCustomers = entity.Customers?.Select(c => new WorkoutCustomerModel
            {
                WorkoutId = c.WorkoutId,
                UserId = c.UserId
            })
        };
    }

    public static CustomerWorkoutsResponse ToCustomerWorkoutsResponse(WorkoutModel model)
    {
        return new CustomerWorkoutsResponse
        {
            Id = model.Id,
            Title = model.Title,
            Description = model.Description,
            StartDate = model.StartDate,
            EndDate = model.EndDate,
            Location = model.Location!.Name,
            MaxBookings = model.MaxBookings,
            Created = model.Created,
            Instructors = model.WorkoutInstructors?.Select(i => new CustomerInstructorResponse
            {
                Id = i.Instructor!.Id,
                FirstName = i.Instructor!.FirstName,
                LastName = i.Instructor!.LastName,
            })
        };
    }
}
