using Business.Dtos.Customer;
using Business.Dtos.Instructor;
using Business.Models;
using System;

namespace Business.Dtos.Workouts;

public class AdminWorkoutResponse
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public DateTime StartDate { get; set; }      
    public DateTime EndDate { get; set; }
    public WorkoutLocationModel Location { get; set; } = null!;
    public int MaxBookings { get; set; }
    public DateTime Created {  get; set; }
    public IEnumerable<InstructorDto> Instructors { get; set; } = [];
    public IEnumerable<CustomerDto> Customers { get; set; } = [];
}
