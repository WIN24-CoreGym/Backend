using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Data.Entities;
public class WorkoutEntity
{
    [Key]
    public int Id { get; set; }

    [Column(TypeName = "nvarchar(50)")]
    public string Title { get; set; } = null!;

    [Column(TypeName = "nvarchar(500)")]
    public string? Description { get; set; }

    [Column(TypeName = "date")]
    public DateTime StartDate { get; set; }

    [Column(TypeName = "date")]
    public DateTime EndDate { get; set; }

    public int LocationId { get; set; }
    public WorkoutLocationEntity? Location { get; set; } = null!;

    public int MaxBookings { get; set; }

    public DateTime Created { get; set; } = DateTime.Now;


    public ICollection<WorkoutInstructorEntity> Instructors { get; set; } = [];
    public ICollection<WorkoutCustomerEntity> Customers { get; set; } = [];
}
