using System;
using System.ComponentModel.DataAnnotations;

namespace Business.Dtos.Workouts;

public class CreateWorkoutRequest
{
    [Required, StringLength(50)]
    public string Title { get; set; } = null!;

    [Required, DataType(DataType.Date)]
    public DateTime Date { get; set; }          

    [Required, DataType(DataType.Time)]
    public TimeSpan Time { get; set; }          

    [Required]
    public int LocationId { get; set; }         

    [Required]
    public int InstructorId { get; set; }      
}
