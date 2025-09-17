namespace Business.Models;
public class WorkoutLocationModel
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;

    public IEnumerable<WorkoutModel>? Workouts { get; set; }
}
