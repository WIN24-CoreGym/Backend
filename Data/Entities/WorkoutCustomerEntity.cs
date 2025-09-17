namespace Data.Entities;
public class WorkoutCustomerEntity
{
    public int WorkoutId { get; set; }
    public string UserId { get; set; } = null!;


    public WorkoutEntity? Workout { get; set; }
}
