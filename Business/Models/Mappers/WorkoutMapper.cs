using System;
using System.Linq;
using Business.Dtos.Workouts;
using Data.Entities;

namespace Business.Models.Mappers;

public static class WorkoutMapper
{

    public static WorkoutEntity ToEntity(this CreateWorkoutRequest req) => new()
    {
        Title = req.Title.Trim(),
        StartDate = req.Date.Date.Add(req.Time), 
        EndDate = req.Date.Date.Add(req.Time),  
        MaxBookings = 20,
        LocationId = req.LocationId,
        Created = DateTime.UtcNow
    };

    public static WorkoutResponse ToResponse(this WorkoutEntity w)
    {
 
        var instr = w.Instructors?.FirstOrDefault()?.Instructor;
        var instructorName =
              (instr?.GetType().GetProperty("FullName")?.GetValue(instr) as string)
           ?? (instr?.GetType().GetProperty("Name")?.GetValue(instr) as string)
           ?? string.Join(" ", new[]
              {
                  instr?.GetType().GetProperty("FirstName")?.GetValue(instr) as string,
                  instr?.GetType().GetProperty("LastName")?.GetValue(instr) as string
              }.Where(s => !string.IsNullOrWhiteSpace(s))).Trim();

        return new WorkoutResponse
        {
            Id = w.Id,
            Title = w.Title,
            Date = w.StartDate.Date,          
            Time = w.StartDate.TimeOfDay,     
            Location = w.Location?.Name ?? "",
            Instructor = instructorName ?? ""
        };
    }
}
