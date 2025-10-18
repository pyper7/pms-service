using System.ComponentModel.DataAnnotations;

namespace PMS.Domain.Entities;

public class Process : BaseEntity
{
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(1000)]
    public string? Description { get; set; }
    
    [Range(0, 100)]
    public int Weight { get; set; }
    
    public bool IsActive { get; set; } = true;
}
