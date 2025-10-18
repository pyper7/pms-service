namespace PMS.Application.Models.AppraisalSettings;

public class ProcessDto
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int Weight { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class CreateProcessRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int Weight { get; set; }
    public bool IsActive { get; set; } = true;
}

public class UpdateProcessRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int Weight { get; set; }
    public bool IsActive { get; set; }
}

public class ProcessListResponse
{
    public List<ProcessDto> Processes { get; set; } = new();
    public PaginationInfo Pagination { get; set; } = new();
}

public class ProcessFilters
{
    public string Search { get; set; } = string.Empty;
    public bool? IsActive { get; set; }
}
