namespace Business.Dtos.Instructor;
public class UpdateInstructorDto
{
    public int Id { get; set; }
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
}
