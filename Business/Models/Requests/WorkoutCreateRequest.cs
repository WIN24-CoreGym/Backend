using System.ComponentModel.DataAnnotations;

public class WorkoutCreateRequest
{
    [Required, StringLength(100)]
    public string Name { get; set; } = null!;

    [Required]
    public DateOnly Date { get; set; }

    [Required]
    public TimeOnly Time { get; set; }

    [Required]
    public int InstructorId { get; set; }

    [Required]
    public int LocationId { get; set; }
}
