namespace Business.Dtos.Instructor;

public class InstructorDto
{
    public int Id { get; set; }
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;

    public IEnumerable<InstructorWorkoutDto> Workouts { get; set; } = [];
}
