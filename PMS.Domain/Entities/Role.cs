using System.ComponentModel.DataAnnotations;

namespace PMS.Domain.Entities;

public class Role : BaseEntity
{
    [Required]
    [StringLength(50)]
    public string Name { get; set; } = string.Empty;
    
    [StringLength(255)]
    public string? Description { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    public ICollection<Post> Posts { get; set; } = new List<Post>();
} 