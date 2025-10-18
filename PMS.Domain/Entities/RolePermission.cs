using System.ComponentModel.DataAnnotations;

namespace PMS.Domain.Entities;

public class RolePermission : BaseEntity
{
    [Required]
    public long RoleId { get; set; }
    
    public Role Role { get; set; } = null!;
    
    [Required]
    public long PermissionId { get; set; }
    
    public Permission Permission { get; set; } = null!;
    
    public DateTime AssignedAt { get; set; }
    
    public string? AssignedBy { get; set; }
}