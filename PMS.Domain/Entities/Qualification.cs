using System.ComponentModel.DataAnnotations;

namespace PMS.Domain.Entities;

public class Qualification : BaseEntity
{
    [Required]
    public long UserId { get; set; }
    
    [Required]
    [MaxLength(200)]
    public string Institution { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(200)]
    public string Degree { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(200)]
    public string Field { get; set; } = string.Empty;
    
    [Required]
    public int Year { get; set; }
    
    [MaxLength(100)]
    public string? Grade { get; set; }
    
    [MaxLength(100)]
    public string? CertificateNumber { get; set; }
    
    // Navigation Properties
    public virtual User User { get; set; } = null!;
}
