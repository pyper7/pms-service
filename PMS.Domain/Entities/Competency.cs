using System.ComponentModel.DataAnnotations;

namespace PMS.Domain.Entities;

public class Competency : BaseEntity
{
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(50)]
    public string Category { get; set; } = string.Empty; // GENERIC, FUNCTIONAL, ETHICS
    
    [MaxLength(1000)]
    public string? Description { get; set; }
    
    public bool IsActive { get; set; } = true;
}
