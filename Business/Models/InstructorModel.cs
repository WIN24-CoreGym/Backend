namespace Business.Models;
public class InstructorModel
{
    public int Id { get; set; }
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;


    public IEnumerable<WorkoutInstructorModel>? WorkoutInstructors { get; set; }
}
