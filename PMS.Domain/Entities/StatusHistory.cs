using System.ComponentModel.DataAnnotations;

namespace PMS.Domain.Entities;

public class StatusHistory : BaseEntity
{
    [Required]
    public long UserId { get; set; }
    
    [Required]
    [MaxLength(20)]
    public string PreviousStatus { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(20)]
    public string NewStatus { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(200)]
    public string Reason { get; set; } = string.Empty;
    
    [Required]
    public DateTime EffectiveDate { get; set; }
    
    [MaxLength(500)]
    public string? Notes { get; set; }
    
    [Required]
    [MaxLength(255)]
    public string ChangedBy { get; set; } = string.Empty;
    
    // Navigation Properties
    public virtual User User { get; set; } = null!;
}
