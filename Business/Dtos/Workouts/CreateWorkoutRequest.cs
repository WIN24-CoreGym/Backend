using System;
using System.ComponentModel.DataAnnotations;

namespace Business.Dtos.Workouts;

public class CreateWorkoutRequest
{
    [Required, StringLength(50)]
    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    [Required]
    public DateTime StartDate { get; set; }          

    [Required]
    public DateTime EndDate { get; set; }

    [Required]
    public string Location { get; set; } = null!;

    [Required]
    public int MaxBookings { get; set; }

    [Required]
    public IEnumerable<int> InstructorIds { get; set; } = [];
}
