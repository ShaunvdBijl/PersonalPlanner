using System.ComponentModel.DataAnnotations;

namespace PersonalPlanner.Data.Models;

public class Category
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid UserId { get; set; }

    public AppUser? User { get; set; }

    [Required, MaxLength(64)]
    public string Name { get; set; } = string.Empty;

    [Required, MaxLength(7)]
    public string ColorHex { get; set; } = "#3B82F6";

    public bool IsPreset { get; set; }

    public ICollection<PlannerEvent> Events { get; set; } = new List<PlannerEvent>();

    public static List<(string Name, string ColorHex)> DefaultPresets =>
    [
        ("Study", "#EF4444"),     // Red
        ("Learning", "#3B82F6"),  // Blue
        ("Exercise", "#EAB308"),  // Yellow
        ("Social", "#22C55E"),    // Green
        ("Meeting", "#F97316")    // Orange
    ];
}