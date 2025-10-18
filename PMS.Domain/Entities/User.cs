using System.ComponentModel.DataAnnotations;

namespace PMS.Domain.Entities;

public class User : BaseEntity
{
    [Required]
    [StringLength(100)]
    public string FirstName { get; set; } = string.Empty;
    
    [Required]
    [StringLength(100)]
    public string LastName { get; set; } = string.Empty;
    
    [Required]
    [StringLength(150)]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
    
    [StringLength(20)]
    public string? PhoneNumber { get; set; }
    
    [StringLength(50)]
    public string? StaffId { get; set; }
    
    [StringLength(10)]
    public string? Gender { get; set; }
    
    public DateTime? DateOfBirth { get; set; }
    
    public DateTime? EmploymentDate { get; set; }
    
    [StringLength(10)]
    public string? GradeLevel { get; set; }
    
    // Additional fields for officer management
    [StringLength(200)]
    public string? Department { get; set; }
    
    [StringLength(200)]
    public string? Position { get; set; }
    
    [StringLength(20)]
    public string? Status { get; set; } = "active";
    
    [StringLength(500)]
    public string? Address { get; set; }
    
    // Emergency Contact
    [StringLength(200)]
    public string? EmergencyContactName { get; set; }
    
    [StringLength(20)]
    public string? EmergencyContactPhone { get; set; }
    
    [StringLength(100)]
    public string? EmergencyContactRelationship { get; set; }
    
    // Status Management
    [StringLength(200)]
    public string? StatusReason { get; set; }
    
    public DateTime? StatusEffectiveDate { get; set; }
    
    [StringLength(500)]
    public string? StatusNotes { get; set; }
    
    // Cadre removed
    
    public bool IsSystemUser { get; set; } = true;
    
    public bool IsActive { get; set; } = true;
    
    public bool IsPostAssigned { get; set; } = false;
    
    [Required]
    [StringLength(50)]
    public string UserType { get; set; } = "Staff";
    
    public string? PasswordHash { get; set; }
    
    public DateTime? LastLoginDate { get; set; }
    
    public ICollection<PostOccupancy> PostOccupancies { get; set; } = new List<PostOccupancy>();
    
    // Officer management related collections
    public ICollection<Qualification> Qualifications { get; set; } = new List<Qualification>();
    public ICollection<WorkHistory> WorkHistory { get; set; } = new List<WorkHistory>();
    public ICollection<StatusHistory> StatusHistory { get; set; } = new List<StatusHistory>();
    
    public string FullName => $"{FirstName} {LastName}".Trim();
    
    public string Name => FullName;
} 
