public class WorkoutResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public DateOnly Date { get; set; }
    public TimeOnly Time { get; set; }
    public int InstructorId { get; set; }
    public int LocationId { get; set; }
}
