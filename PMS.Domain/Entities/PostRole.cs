using System.ComponentModel.DataAnnotations;

namespace PMS.Domain.Entities;

public class PostRole : BaseEntity
{
    
    
    [Required]
    public long PostId { get; set; }
    
    public Post Post { get; set; } = null!;
    
    [Required]
    public long RoleId { get; set; }
    
    public Role Role { get; set; } = null!;
} 