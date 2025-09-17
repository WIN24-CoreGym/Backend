using System.ComponentModel.DataAnnotations;

namespace Data.Entities;
public class WorkoutLocationEntity
{
    [Key]
    public int Id { get; set; }

    public string Name { get; set; } = null!;


    public ICollection<WorkoutEntity> Workouts { get; set; } = [];
}
