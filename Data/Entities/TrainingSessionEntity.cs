using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Data.Entities;

public class TrainingSessionEntity
{
    [Key]
    public int Id { get; set; }

    [Required, Column(TypeName = "nvarchar(100)")]
    public string Name { get; set; } = null!;

    [Required, Column(TypeName = "date")]
    public DateTime Date { get; set; }

    [Required, Column(TypeName = "time")]
    public TimeSpan Time { get; set; }

    [Required, Column(TypeName = "nvarchar(100)")]
    public string Instructor { get; set; } = null!;

    [Required, Column(TypeName = "nvarchar(150)")]
    public string Location { get; set; } = null!;

    public DateTime CreatedAt { get; set; } = DateTime.Now;
}
