namespace Business.Dtos.Workouts;
public class UpdateWorkoutRequest
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public IEnumerable<int> InstructorIds { get; set; } = [];
    public string Location { get; set; } = null!;
    public int MaxBookings { get; set; }
    public DateTime Created { get; set; }
}
