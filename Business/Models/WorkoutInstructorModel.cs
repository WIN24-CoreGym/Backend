namespace Business.Models;
public class WorkoutInstructorModel
{
    public int WorkoutId { get; set; }
    public int InstructorId { get; set; }


    public InstructorModel? Instructor { get; set; }
    public WorkoutModel? Workout { get; set; }
}
