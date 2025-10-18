using System.ComponentModel.DataAnnotations;

namespace PMS.Domain.Entities;

public class Permission : BaseEntity
{
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [StringLength(255)]
    public string? Description { get; set; }
    
    [StringLength(50)]
    public string? Module { get; set; }
    
    public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
} 