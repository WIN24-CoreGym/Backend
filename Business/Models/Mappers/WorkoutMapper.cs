using Business.Dtos.Instructor;
using Business.Dtos.Workouts;
using Business.Dtos.Customer;
using Data.Entities;
using Microsoft.IdentityModel.Tokens;

namespace Business.Models.Mappers;

public static class WorkoutMapper
{

    public static WorkoutEntity ToEntity(this CreateWorkoutRequest req) => new()
    {
        Title = req.Title.Trim(),
        Description = req.Description,
        StartDate = req.StartDate, 
        EndDate = req.EndDate, 
        MaxBookings = req.MaxBookings,
        Created = DateTime.UtcNow
    };

    public static WorkoutModel ToModel(this WorkoutEntity w) => new()
    {
        Id = w.Id,
        Title = w.Title,
        Description = w.Description,
        StartDate = w.StartDate,
        EndDate = w.EndDate,
        Location = new WorkoutLocationModel { Id = w.Location!.Id, Name = w.Location.Name },
        WorkoutInstructors = w.Instructors?.Select(x => new WorkoutInstructorModel
        {
            WorkoutId = x.WorkoutId,
            InstructorId = x.InstructorId,
            Instructor = x.Instructor == null ? null : new InstructorModel
            {
                Id = x.Instructor.Id,
                FirstName = x.Instructor.FirstName,
                LastName = x.Instructor.LastName,
            }
        }).ToList() ?? [],
        MaxBookings = w.MaxBookings,
        Created = w.Created,
        WorkoutCustomers = w.Customers.Select(x => new WorkoutCustomerModel
        {
            WorkoutId = x.WorkoutId,
            UserId = x.UserId,
            Customer = x.Customer == null ? null : new CustomerModel
            {
                UserId = x.Customer.Id,
                FirstName = x.Customer.FirstName,
                LastName = x.Customer.LastName,
                Email = x.Customer.Email!
            }
        }).ToList() ?? [],
    };

    public static CustomerWorkoutResponse ToCustomerResponse(this WorkoutModel w) => new()
    {
        Id = w.Id,
        Title = w.Title,
        Description = w.Description,
        StartDate = w.StartDate,
        EndDate = w.EndDate,
        Location = new WorkoutLocationModel { Id = w.Location!.Id, Name = w.Location.Name },
        Instructors = w.WorkoutInstructors?.Select(x => new CustomerInstructorResponse
        {
            Id = x.Instructor!.Id,
            FirstName = x.Instructor.FirstName,
            LastName = x.Instructor.LastName,
        }).ToList() ?? [],
        MaxBookings = w.MaxBookings,
        Created = w.Created,
    };

    public static AdminWorkoutResponse ToAdminResponse(this WorkoutModel w) => new()
    {
        Id = w.Id,
        Title = w.Title,
        Description = w.Description,
        StartDate = w.StartDate,
        EndDate = w.EndDate,
        Location = new WorkoutLocationModel { Id = w.Location!.Id, Name = w.Location.Name },
        Instructors = w.WorkoutInstructors?.Where(x => x.Instructor != null).Select(x => new InstructorDto
        {
            Id = x.Instructor!.Id,
            FirstName = x.Instructor.FirstName,
            LastName = x.Instructor.LastName,
        }).ToList() ?? [],
        MaxBookings = w.MaxBookings,
        Created = w.Created,
        Customers = w.WorkoutCustomers?.Where(x => x.Customer != null).Select(x => new CustomerDto
        {
            UserId = x.Customer!.UserId,
            FirstName = x.Customer.FirstName,
            LastName = x.Customer.LastName,
            Email = x.Customer.Email!
        }).ToList() ?? [],
    };
}
