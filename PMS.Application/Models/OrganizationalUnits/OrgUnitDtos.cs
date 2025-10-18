using System.ComponentModel.DataAnnotations;
using PMS.Application.Models.Posts;

namespace PMS.Application.Models.OrganizationalUnits;

// Core DTOs
public class OrgUnitDto
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string? Description { get; set; }
    public long? ParentId { get; set; }
    public int Level { get; set; }
    public int Order { get; set; }
    public string Status { get; set; } = string.Empty;
    public HeadOfUnitDto? HeadOfUnit { get; set; }
    public int ChildrenCount { get; set; }
    public int PostsCount { get; set; }
    public int StaffCount { get; set; }
    public DateTime DateCreated { get; set; }
    public DateTime LastUpdated { get; set; }
}

public class HeadOfUnitDto
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Position { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? StaffId { get; set; }
    public DateTime? EffectiveDate { get; set; }
    public DateTime? EndDate { get; set; }
}

// List DTOs
public class OrgUnitListDto
{
    public List<OrgUnitDto> Units { get; set; } = new();
    public PaginationDto Pagination { get; set; } = new();
    public OrgUnitSummaryDto Summary { get; set; } = new();
}

public class OrgUnitSummaryDto
{
    public int TotalUnits { get; set; }
    public int ActiveUnits { get; set; }
    public int InactiveUnits { get; set; }
    public Dictionary<string, int> ByType { get; set; } = new();
}

// Tree DTOs
public class OrgUnitTreeDto
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public int Level { get; set; }
    public int Order { get; set; }
    public List<OrgUnitTreeDto> Children { get; set; } = new();
}

public class OrgUnitTreeResponseDto
{
    public List<OrgUnitTreeDto> Tree { get; set; } = new();
    public OrgUnitTreeSummaryDto Summary { get; set; } = new();
}

public class OrgUnitTreeSummaryDto
{
    public int TotalUnits { get; set; }
    public int MaxDepth { get; set; }
    public Dictionary<string, int> ByLevel { get; set; } = new();
}

// Request DTOs
public class CreateOrgUnitRequest
{
    [Required]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;

    [Required]
    public string Type { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }

    public long? ParentId { get; set; }

    public int Order { get; set; } = 1;
}

public class UpdateOrgUnitRequest
{
    [StringLength(200)]
    public string? Name { get; set; }

    [StringLength(500)]
    public string? Description { get; set; }

    public int? Order { get; set; }
}

public class MoveOrgUnitRequest
{
    [Required]
    public long NewParentId { get; set; }

    public int NewOrder { get; set; } = 1;
}

// Head of Unit DTOs
public class AssignHeadRequest
{
    [Required]
    public string StaffId { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string Position { get; set; } = string.Empty;

    [Required]
    public DateTime EffectiveDate { get; set; }
}

public class RemoveHeadRequest
{
    [Required]
    public DateTime EffectiveDate { get; set; }

    [StringLength(500)]
    public string? Reason { get; set; }
}

public class HeadOfUnitHistoryDto
{
    public long Id { get; set; }
    public string StaffId { get; set; } = string.Empty;
    public string StaffName { get; set; } = string.Empty;
    public string Position { get; set; } = string.Empty;
    public DateTime EffectiveDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Reason { get; set; }
}

// Statistics DTOs
public class OrgUnitStatisticsDto
{
    public OrgUnitOverviewDto Overview { get; set; } = new();
    public Dictionary<string, OrgUnitTypeStatsDto> ByType { get; set; } = new();
    public Dictionary<string, OrgUnitLevelStatsDto> ByLevel { get; set; } = new();
    public OrgUnitHierarchyDto Hierarchy { get; set; } = new();
}

public class OrgUnitOverviewDto
{
    public int TotalUnits { get; set; }
    public int ActiveUnits { get; set; }
    public int InactiveUnits { get; set; }
    public int TotalStaff { get; set; }
    public int TotalPosts { get; set; }
    public int VacantPosts { get; set; }
}

public class OrgUnitTypeStatsDto
{
    public int Count { get; set; }
    public int Staff { get; set; }
    public int Posts { get; set; }
    public int VacantPosts { get; set; }
}

public class OrgUnitLevelStatsDto
{
    public int Units { get; set; }
    public int Staff { get; set; }
    public int Posts { get; set; }
}

public class OrgUnitHierarchyDto
{
    public int MaxDepth { get; set; }
    public double AverageChildrenPerUnit { get; set; }
    public int UnitsWithChildren { get; set; }
    public int LeafUnits { get; set; }
}

// Performance Metrics DTOs
public class OrgUnitMetricsDto
{
    public long UnitId { get; set; }
    public string UnitName { get; set; } = string.Empty;
    public OrgUnitMetricsDataDto Metrics { get; set; } = new();
    public OrgUnitTrendsDto Trends { get; set; } = new();
}

public class OrgUnitMetricsDataDto
{
    public OrgUnitStaffingDto Staffing { get; set; } = new();
    public OrgUnitPostsDto Posts { get; set; } = new();
    public OrgUnitPerformanceDto Performance { get; set; } = new();
    public OrgUnitHierarchyMetricsDto Hierarchy { get; set; } = new();
}

public class OrgUnitStaffingDto
{
    public int TotalStaff { get; set; }
    public int ActiveStaff { get; set; }
    public int InactiveStaff { get; set; }
    public double StaffTurnoverRate { get; set; }
    public double AverageTenure { get; set; }
}

public class OrgUnitPostsDto
{
    public int TotalPosts { get; set; }
    public int OccupiedPosts { get; set; }
    public int VacantPosts { get; set; }
    public double OccupancyRate { get; set; }
}

public class OrgUnitPerformanceDto
{
    public double AverageAppraisalScore { get; set; }
    public int CompletedAppraisals { get; set; }
    public int PendingAppraisals { get; set; }
    public double CompletionRate { get; set; }
}

public class OrgUnitHierarchyMetricsDto
{
    public int ChildrenCount { get; set; }
    public int MaxDepth { get; set; }
    public double AverageChildrenPerChild { get; set; }
}

public class OrgUnitTrendsDto
{
    public OrgUnitGrowthDto StaffGrowth { get; set; } = new();
    public OrgUnitPerformanceTrendDto PerformanceTrend { get; set; } = new();
}

public class OrgUnitGrowthDto
{
    public int LastMonth { get; set; }
    public int LastQuarter { get; set; }
    public int LastYear { get; set; }
}

public class OrgUnitPerformanceTrendDto
{
    public double LastMonth { get; set; }
    public double LastQuarter { get; set; }
    public double LastYear { get; set; }
}

// Bulk Operations DTOs
public class BulkCreateOrgUnitsRequest
{
    [Required]
    public List<CreateOrgUnitRequest> Units { get; set; } = new();
}

public class BulkUpdateOrgUnitsRequest
{
    [Required]
    public List<BulkUpdateOrgUnitDto> Updates { get; set; } = new();
}

public class BulkUpdateOrgUnitDto
{
    [Required]
    public long Id { get; set; }

    [StringLength(200)]
    public string? Name { get; set; }

    [StringLength(500)]
    public string? Description { get; set; }

    public int? Order { get; set; }
}

public class BulkOperationResultDto
{
    public int Created { get; set; }
    public int Updated { get; set; }
    public int Failed { get; set; }
    public List<BulkOperationItemDto> Results { get; set; } = new();
}

public class BulkOperationItemDto
{
    public int Index { get; set; }
    public bool Success { get; set; }
    public long? UnitId { get; set; }
    public string? Name { get; set; }
    public string? Error { get; set; }
}

// Export DTOs
public class ExportResponseDto
{
    public string DownloadUrl { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public DateTime ExpiresAt { get; set; }
}

// Validation DTOs
public class ValidateOrgUnitRequest
{
    [Required]
    public string Name { get; set; } = string.Empty;

    [Required]
    public string Type { get; set; } = string.Empty;

    public long? ParentId { get; set; }
}

public class ValidationResultDto
{
    public bool Valid { get; set; }
    public List<string> Warnings { get; set; } = new();
    public List<string> Errors { get; set; } = new();
    public List<ValidationSuggestionDto> Suggestions { get; set; } = new();
}

public class ValidationSuggestionDto
{
    public string Field { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}

public class NameAvailabilityDto
{
    public bool Available { get; set; }
    public List<string> Suggestions { get; set; } = new();
}

// Delete Response DTOs
public class DeleteOrgUnitResponseDto
{
    public long DeletedUnitId { get; set; }
    public int DeletedChildrenCount { get; set; }
    public int AffectedPostsCount { get; set; }
    public int AffectedStaffCount { get; set; }
}

// Move Response DTOs
public class MoveOrgUnitResponseDto
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public long NewParentId { get; set; }
    public int NewLevel { get; set; }
    public int NewOrder { get; set; }
    public int AffectedChildrenCount { get; set; }
}

// Head Assignment Response DTOs
public class AssignHeadResponseDto
{
    public long UnitId { get; set; }
    public string UnitName { get; set; } = string.Empty;
    public HeadOfUnitDto HeadOfUnit { get; set; } = new();
}

public class RemoveHeadResponseDto
{
    public long UnitId { get; set; }
    public string UnitName { get; set; } = string.Empty;
    public HeadOfUnitDto RemovedHead { get; set; } = new();
}
