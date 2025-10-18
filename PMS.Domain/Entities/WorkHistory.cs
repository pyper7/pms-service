using System.ComponentModel.DataAnnotations;

namespace PMS.Domain.Entities;

public class WorkHistory : BaseEntity
{
    [Required]
    public long UserId { get; set; }
    
    [Required]
    [MaxLength(200)]
    public string Position { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(200)]
    public string Department { get; set; } = string.Empty;
    
    [Required]
    public DateTime StartDate { get; set; }
    
    public DateTime? EndDate { get; set; }
    
    [MaxLength(500)]
    public string? Notes { get; set; }
    
    // Navigation Properties
    public virtual User User { get; set; } = null!;
}
