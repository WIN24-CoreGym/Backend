namespace Business.Models;
public class WorkoutCustomerModel
{
    public int WorkoutId { get; set; }
    public string UserId { get; set; } = null!;

    public WorkoutModel? Workout { get; set; }
}
