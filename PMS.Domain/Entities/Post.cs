using System.ComponentModel.DataAnnotations;

namespace PMS.Domain.Entities;

public class Post : BaseEntity
{
    [Required]
    public long OrgUnitId { get; set; }
    
    public OrgUnit OrgUnit { get; set; } = null!;
    
    [Required]
    public long RoleId { get; set; }
    
    public Role Role { get; set; } = null!;
    
    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;
    
    [StringLength(500)]
    public string? Description { get; set; }
    
    [Required]
    [StringLength(10)]
    public string GradeLevel { get; set; } = string.Empty;
    
    public bool IsUnique { get; set; } = true;
    
    public PostStatus Status { get; set; } = PostStatus.Active;
    
    public ICollection<PostOccupancy> PostOccupancies { get; set; } = new List<PostOccupancy>();
}

public enum PostStatus
{
    Active,
    Archived,
    Suspended,
    UnderReview
} 