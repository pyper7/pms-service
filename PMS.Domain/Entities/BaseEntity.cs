using System.ComponentModel.DataAnnotations;

namespace PMS.Domain.Entities;

public abstract class BaseEntity
{
    [Key]
    public long Id { get; set; }
    
    public DateTime CreatedAt { get; set; }
    
    public DateTime? UpdatedAt { get; set; }
    
    public string CreatedBy { get; set; } = string.Empty;
    
    public string? UpdatedBy { get; set; }
    
    public bool IsDeleted { get; set; }
    
    protected BaseEntity()
    {
        CreatedAt = DateTime.UtcNow;
        IsDeleted = false;
    }
} 