namespace Data.Entities;
public class WorkoutCustomerEntity
{
    public int WorkoutId { get; set; }
    public string UserId { get; set; } = null!;
    public DateTime BookingDate { get; set; } = DateTime.Now;


    public AppUser? Customer { get; set; }
    public WorkoutEntity? Workout { get; set; }
}
