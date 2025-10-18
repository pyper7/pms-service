using System.ComponentModel.DataAnnotations;

namespace PMS.Domain.Entities;

public class PostOccupancyRequest : BaseEntity
{
    
    [Required]
    public long UserId { get; set; }
    
    public User User { get; set; } = null!;
    
    [Required]
    public long PostId { get; set; }
    
    public Post Post { get; set; } = null!;
    
    [Required]
    public long OrgUnitId { get; set; }
    
    public OrgUnit OrgUnit { get; set; } = null!;
    
    [StringLength(1000)]
    public string? Notes { get; set; }
    
    [StringLength(1000)]
    public string? Reason { get; set; }
    
    public RequestStatus Status { get; set; } = RequestStatus.Pending;
    
    [Required]
    public DateTime RequestedAt { get; set; }
    
    public DateTime? ApprovedAt { get; set; }
    
    public long? ApprovedBy { get; set; }
    
    public User? ApprovalUser { get; set; }
    
    public bool IsPending => Status == RequestStatus.Pending;
    
    public bool IsApproved => Status == RequestStatus.Approved;
    
    public bool IsRejected => Status == RequestStatus.Rejected;
}

public enum RequestStatus
{
    Pending,
    Approved,
    Rejected,
    UnderReview,
    Cancelled
} 