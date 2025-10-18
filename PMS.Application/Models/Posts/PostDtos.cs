namespace PMS.Application.Models.Posts;

public class PostDto
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string GradeLevel { get; set; } = string.Empty;
    public long OrgUnitId { get; set; }
    public string OrgUnitName { get; set; } = string.Empty;
    public string? OrgUnitType { get; set; }
    public string Status { get; set; } = string.Empty;
    public bool IsOccupied { get; set; }
    public long? AssignedOfficerId { get; set; }
    public string? AssignedOfficerName { get; set; }
    public long? RoleId { get; set; }
    public string? RoleName { get; set; }
    public DateTime DateCreated { get; set; }
    public DateTime LastUpdated { get; set; }
}

public class PostListDto
{
    public List<PostDto> Posts { get; set; } = new();
    public PaginationDto Pagination { get; set; } = new();
}

public class PaginationDto
{
    public int Total { get; set; }
    public int Page { get; set; }
    public int Limit { get; set; }
    public int TotalPages { get; set; }
    public bool HasNext { get; set; }
    public bool HasPrevious { get; set; }
}

public class PostOccupancyDto
{
    public long Id { get; set; }
    public string PostName { get; set; } = string.Empty;
    public long OfficerId { get; set; }
    public string OfficerName { get; set; } = string.Empty;
    public string? StaffId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool IsActive { get; set; }
    public string? Notes { get; set; }
    public DateTime DateCreated { get; set; }
    public DateTime LastUpdated { get; set; }
}

public class PostOccupancyListDto
{
    public List<PostOccupancyDto> Occupancies { get; set; } = new();
    public PaginationDto Pagination { get; set; } = new();
}

public class CreatePostRequest
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string GradeLevel { get; set; } = string.Empty;
    public long OrgUnitId { get; set; }
    public string Status { get; set; } = "active";
    public long? RoleId { get; set; }
}

public class UpdatePostRequest
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string GradeLevel { get; set; } = string.Empty;
    public string Status { get; set; } = "active";
}

public class AssignRoleToPostRequest
{
    public long RoleId { get; set; }
}

public class CreatePostOccupancyRequest
{
    public long PostId { get; set; }
    public long OfficerId { get; set; }
    public string? Notes { get; set; }
    public DateTime StartDate { get; set; }
}

public class UpdatePostOccupancyRequest
{
    public string? Notes { get; set; }
    public bool IsActive { get; set; }
}

public class EndPostOccupancyRequest
{
    public DateTime EndDate { get; set; }
    public string? Notes { get; set; }
}

public class BulkCreatePostsRequest
{
    public List<CreatePostRequest> Posts { get; set; } = new();
}

public class BulkAssignOfficersRequest
{
    public List<CreatePostOccupancyRequest> Assignments { get; set; } = new();
}

public class ValidateAssignmentRequest
{
    public long OfficerId { get; set; }
}

public class AssignmentValidationDto
{
    public bool IsValid { get; set; }
    public List<string> Conflicts { get; set; } = new();
    public List<ValidationWarningDto> Warnings { get; set; } = new();
}

public class ValidationWarningDto
{
    public string Type { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}

public class PostAvailabilityDto
{
    public bool IsAvailable { get; set; }
    public PostOccupancyDto? CurrentOccupancy { get; set; }
    public bool CanAssign { get; set; }
    public string? Reason { get; set; }
}

public class PostStatisticsDto
{
    public int TotalPosts { get; set; }
    public int ActivePosts { get; set; }
    public int InactivePosts { get; set; }
    public int OccupiedPosts { get; set; }
    public int VacantPosts { get; set; }
    public List<GradeLevelCountDto> PostsByGradeLevel { get; set; } = new();
    public List<OrgUnitCountDto> PostsByOrgUnit { get; set; } = new();
}

public class GradeLevelCountDto
{
    public string GradeLevel { get; set; } = string.Empty;
    public int Count { get; set; }
}


public class OrgUnitCountDto
{
    public long OrgUnitId { get; set; }
    public string OrgUnitName { get; set; } = string.Empty;
    public int Count { get; set; }
}

public class OccupancyStatisticsDto
{
    public int TotalAssignments { get; set; }
    public int ActiveAssignments { get; set; }
    public int EndedAssignments { get; set; }
    public int AverageAssignmentDuration { get; set; }
    public List<MonthlyCountDto> AssignmentsByMonth { get; set; } = new();
    public List<TopOccupiedPostDto> TopOccupiedPosts { get; set; } = new();
}

public class MonthlyCountDto
{
    public string Month { get; set; } = string.Empty;
    public int Count { get; set; }
}

public class TopOccupiedPostDto
{
    public long PostId { get; set; }
    public string PostName { get; set; } = string.Empty;
    public int AssignmentCount { get; set; }
}
