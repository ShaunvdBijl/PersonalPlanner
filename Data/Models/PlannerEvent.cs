using System.ComponentModel.DataAnnotations;

namespace PersonalPlanner.Data.Models;

public class PlannerEvent
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid UserId { get; set; }

    public AppUser? User { get; set; }

    [Required]
    public Guid CategoryId { get; set; }

    public Category? Category { get; set; }

    [Required, MaxLength(120)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    [Required]
    public DateOnly EventDate { get; set; }

    public TimeOnly? StartTime { get; set; }
    public TimeOnly? EndTime { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}