using System.ComponentModel.DataAnnotations;

namespace PMS.Domain.Entities;

public class ScoringWeight : BaseEntity
{
    [Required]
    [MaxLength(50)]
    public string Section { get; set; } = string.Empty; // KPI_TASKS, COMPETENCIES, PROCESSES
    
    [Range(0, 100)]
    public int Weight { get; set; }
    
    [MaxLength(200)]
    public string? Description { get; set; }
}
