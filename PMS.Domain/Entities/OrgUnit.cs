using System.ComponentModel.DataAnnotations;

namespace PMS.Domain.Entities;

public class OrgUnit : BaseEntity
{
    [Required]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;
    
    public OrgUnitType Type { get; set; }
    
    public long? ParentId { get; set; }
    
    public OrgUnit? Parent { get; set; }
    
    public ICollection<OrgUnit> Children { get; set; } = new List<OrgUnit>();
    
    [StringLength(500)]
    public string? Description { get; set; }
    
    [StringLength(50)]
    public string? Code { get; set; }
    
    public int Level { get; set; } = 0;
    
    public int Order { get; set; } = 1;
    
    public bool IsActive { get; set; } = true;
    
    // Navigation properties
    public ICollection<Post> Posts { get; set; } = new List<Post>();
    public ICollection<OrgUnitHead> HeadHistory { get; set; } = new List<OrgUnitHead>();
}

public enum OrgUnitType
{
    BOARD = 1,
    EXECUTIVE,
    DEPT,
    DIV,
    BRANCH
}

public class OrgUnitHead : BaseEntity
{
    public long OrgUnitId { get; set; }
    public OrgUnit? OrgUnit { get; set; }
    
    public long UserId { get; set; }
    public User? User { get; set; }
    
    [Required]
    [StringLength(100)]
    public string Position { get; set; } = string.Empty;
    
    public DateTime EffectiveDate { get; set; }
    
    public DateTime? EndDate { get; set; }
    
    [StringLength(500)]
    public string? Reason { get; set; }
    
    public bool IsActive { get; set; } = true;
} 