using Data.Entities;

namespace Business.Models;
public class WorkoutCustomerModel
{
    public int WorkoutId { get; set; }
    public string UserId { get; set; } = null!;

    public WorkoutModel? Workout { get; set; }
    public CustomerModel? Customer {  get; set; }
}
