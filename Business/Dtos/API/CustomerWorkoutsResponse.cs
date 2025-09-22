using Business.Dtos.API;
using Business.Models;

namespace WebApi.Dtos;

public class CustomerWorkoutsResponse
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string Location { get; set; } = null!;
    public int MaxBookings { get; set; }
    public DateTime Created { get; set; }


    public IEnumerable<CustomerInstructorResponse>? Instructors { get; set; }
}
