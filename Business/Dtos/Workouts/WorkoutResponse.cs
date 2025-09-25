using System;

namespace Business.Dtos.Workouts;

public class WorkoutResponse
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public DateTime Date { get; set; }      
    public TimeSpan Time { get; set; }    
    public string Location { get; set; } = null!;
    public string Instructor { get; set; } = null!;
}
