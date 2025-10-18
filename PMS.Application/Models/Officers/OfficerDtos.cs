using PMS.Application.Models.Posts;
using System.ComponentModel.DataAnnotations;

namespace PMS.Application.Models.Officers;

public class OfficerDto
{
    public long Id { get; set; }
    public string StaffId { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public string Position { get; set; } = string.Empty;
    public string GradeLevel { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime? DateOfBirth { get; set; }
    public DateTime? DateOfEmployment { get; set; }
    public string? Address { get; set; }
    public EmergencyContactDto? EmergencyContact { get; set; }
    public List<QualificationDto> Qualifications { get; set; } = new();
    public List<WorkHistoryDto> WorkHistory { get; set; } = new();
    public CurrentPostDto? CurrentPost { get; set; }
    public PerformanceHistoryDto? PerformanceHistory { get; set; }
    public DateTime DateCreated { get; set; }
    public DateTime LastUpdated { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public string UpdatedBy { get; set; } = string.Empty;
}

public class OfficerListDto
{
    public List<OfficerDto> Officers { get; set; } = new();
    public PaginationDto Pagination { get; set; } = new();
}

public class EmergencyContactDto
{
    public string Name { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Relationship { get; set; } = string.Empty;
}

public class QualificationDto
{
    public long Id { get; set; }
    public long UserId { get; set; }
    public string Institution { get; set; } = string.Empty;
    public string Degree { get; set; } = string.Empty;
    public string Field { get; set; } = string.Empty;
    public int Year { get; set; }
    public string? Grade { get; set; }
    public string? CertificateNumber { get; set; }
    public DateTime DateCreated { get; set; }
    public DateTime? LastUpdated { get; set; }
}

public class WorkHistoryDto
{
    public long Id { get; set; }
    public long UserId { get; set; }
    public string Position { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? Notes { get; set; }
    public DateTime DateCreated { get; set; }
}

public class CurrentPostDto
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string OrgUnit { get; set; } = string.Empty;
    public DateTime AssignedDate { get; set; }
}

public class PerformanceHistoryDto
{
    public int TotalAppraisals { get; set; }
    public double AverageRating { get; set; }
    public DateTime? LastAppraisalDate { get; set; }
}

// Request Models
public class CreateOfficerRequest
{
    [Required]
    public string StaffId { get; set; } = string.Empty;
    
    [Required]
    public string FirstName { get; set; } = string.Empty;
    
    [Required]
    public string LastName { get; set; } = string.Empty;
    
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
    
    public string? PhoneNumber { get; set; }
    
    public string? Department { get; set; }
    
    public string? Position { get; set; }
    
    public string? GradeLevel { get; set; }
    
    public string? Status { get; set; }
    
    public DateTime? DateOfBirth { get; set; }
    public DateTime? DateOfEmployment { get; set; }
    public string? Address { get; set; }
    public EmergencyContactDto? EmergencyContact { get; set; }
    public List<CreateQualificationRequest> Qualifications { get; set; } = new();
}

public class UpdateOfficerRequest
{
    [Required]
    public string FirstName { get; set; } = string.Empty;
    
    [Required]
    public string LastName { get; set; } = string.Empty;
    
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
    
    public string? PhoneNumber { get; set; }
    
    public string? Department { get; set; }
    
    public string? Position { get; set; }
    
    public string? GradeLevel { get; set; }
    
    public string? Status { get; set; }
    
    public DateTime? DateOfBirth { get; set; }
    public DateTime? DateOfEmployment { get; set; }
    public string? Address { get; set; }
    public EmergencyContactDto? EmergencyContact { get; set; }
}

public class CreateQualificationRequest
{
    [Required]
    public string Institution { get; set; } = string.Empty;
    
    [Required]
    public string Degree { get; set; } = string.Empty;
    
    [Required]
    public string Field { get; set; } = string.Empty;
    
    [Required]
    public int Year { get; set; }
    
    public string? Grade { get; set; }
    public string? CertificateNumber { get; set; }
}

public class UpdateQualificationRequest
{
    [Required]
    public string Institution { get; set; } = string.Empty;
    
    [Required]
    public string Degree { get; set; } = string.Empty;
    
    [Required]
    public string Field { get; set; } = string.Empty;
    
    [Required]
    public int Year { get; set; }
    
    public string? Grade { get; set; }
    public string? CertificateNumber { get; set; }
}

public class CreateWorkHistoryRequest
{
    [Required]
    public string Position { get; set; } = string.Empty;
    
    [Required]
    public string Department { get; set; } = string.Empty;
    
    [Required]
    public DateTime StartDate { get; set; }
    
    public DateTime? EndDate { get; set; }
    public string? Notes { get; set; }
}

public class UpdateOfficerStatusRequest
{
    [Required]
    public string Status { get; set; } = string.Empty;
    
    [Required]
    public string Reason { get; set; } = string.Empty;
    
    [Required]
    public DateTime EffectiveDate { get; set; }
    
    public string? Notes { get; set; }
}

public class StatusHistoryDto
{
    public long Id { get; set; }
    public long UserId { get; set; }
    public string PreviousStatus { get; set; } = string.Empty;
    public string NewStatus { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public DateTime EffectiveDate { get; set; }
    public string? Notes { get; set; }
    public string ChangedBy { get; set; } = string.Empty;
    public DateTime DateChanged { get; set; }
}

// Bulk Operations
public class BulkCreateOfficersRequest
{
    [Required]
    public List<CreateOfficerRequest> Officers { get; set; } = new();
}

public class BulkUpdateOfficersRequest
{
    [Required]
    public List<BulkUpdateOfficerRequest> Updates { get; set; } = new();
}

public class BulkUpdateOfficerRequest
{
    [Required]
    public long Id { get; set; }
    public string? Department { get; set; }
    public string? GradeLevel { get; set; }
    public string? Position { get; set; }
    public string? Status { get; set; }
}

public class BulkDeleteOfficersRequest
{
    [Required]
    public List<long> Ids { get; set; } = new();
}

// Search and Statistics
public class AdvancedSearchRequest
{
    public SearchCriteriaDto? Criteria { get; set; }
    public PaginationDto? Pagination { get; set; }
    public SortingDto? Sorting { get; set; }
}

public class SearchCriteriaDto
{
    public string? Name { get; set; }
    public string? Department { get; set; }
    public GradeLevelRangeDto? GradeLevel { get; set; }
    public DateRangeDto? DateOfEmployment { get; set; }
    public QualificationSearchDto? Qualifications { get; set; }
}

public class GradeLevelRangeDto
{
    public int? Min { get; set; }
    public int? Max { get; set; }
}

public class DateRangeDto
{
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
}

public class QualificationSearchDto
{
    public string? Degree { get; set; }
    public string? Field { get; set; }
}

public class SortingDto
{
    public string Field { get; set; } = "name";
    public string Order { get; set; } = "asc";
}

public class OfficerStatisticsDto
{
    public int TotalOfficers { get; set; }
    public int ActiveOfficers { get; set; }
    public int InactiveOfficers { get; set; }
    public Dictionary<string, int> ByDepartment { get; set; } = new();
    public Dictionary<string, int> ByGradeLevel { get; set; } = new();
    public Dictionary<string, int> ByStatus { get; set; } = new();
    public double AverageAge { get; set; }
    public double AverageYearsOfService { get; set; }
    public int NewHiresThisYear { get; set; }
    public int RetirementsThisYear { get; set; }
}

public class ValidationResultDto
{
    public bool IsValid { get; set; }
    public List<ValidationErrorDto> Errors { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
}

public class ValidationErrorDto
{
    public string Field { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
}

public class ExportResultDto
{
    public string DownloadUrl { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string FileSize { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
}

public class SearchMetadataDto
{
    public string SearchTime { get; set; } = string.Empty;
    public int TotalMatches { get; set; }
    public int FiltersApplied { get; set; }
}
