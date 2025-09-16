namespace Data.Entities;
public class WorkoutInstructorEntity
{
    public int WorkoutId { get; set; }
    public int InstructorId { get; set; }


    public InstructorEntity? Instructor { get; set; }
    public WorkoutEntity? Workout { get; set; }
}
