using System.ComponentModel.DataAnnotations;

namespace PMS.Domain.Entities;

public class PostOccupancy : BaseEntity
{
    [Required]
    public long PostId { get; set; }
    
    public Post Post { get; set; } = null!;
    
    [Required]
    public long UserId { get; set; }
    
    public User User { get; set; } = null!;
    
    [Required]
    public DateTime StartDate { get; set; }
    
    public DateTime? EndDate { get; set; }
    
    public bool IsPrimary { get; set; } = true;
    
    [StringLength(500)]
    public string? Remarks { get; set; }
    
    public bool IsCurrentlyOccupying => EndDate == null;
} 