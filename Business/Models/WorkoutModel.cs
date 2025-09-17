namespace Business.Models;
public class WorkoutModel
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int LocationId { get; set; }
    public int MaxBookings { get; set; }
    public DateTime Created {  get; set; }


    public WorkoutLocationModel? Location { get; set; }
    public IEnumerable<WorkoutInstructorModel>? WorkoutInstructors { get; set; }
    public IEnumerable<WorkoutCustomerModel>? WorkoutCustomers { get; set; }
}
