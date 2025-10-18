namespace PMS.Application.Models.AppraisalSettings;

public class CompetencyDto
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class CreateCompetencyRequest
{
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
}

public class UpdateCompetencyRequest
{
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
}

public class CompetencyListResponse
{
    public List<CompetencyDto> Competencies { get; set; } = new();
    public PaginationInfo Pagination { get; set; } = new();
    public CompetencyFilters Filters { get; set; } = new();
}

public class CompetencyFilters
{
    public string Search { get; set; } = string.Empty;
    public string Category { get; set; } = "ALL";
    public bool? IsActive { get; set; }
}

public class BulkCreateCompetencyRequest
{
    public List<CreateCompetencyRequest> Competencies { get; set; } = new();
}

public class BulkUpdateCompetencyRequest
{
    public List<BulkUpdateCompetencyItem> Competencies { get; set; } = new();
}

public class BulkUpdateCompetencyItem
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
}

public class BulkOperationResponse
{
    public int Created { get; set; }
    public int Updated { get; set; }
    public int Failed { get; set; }
    public List<CompetencyDto> Competencies { get; set; } = new();
    public List<BulkError> Errors { get; set; } = new();
}

public class BulkError
{
    public int Row { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Error { get; set; } = string.Empty;
}
