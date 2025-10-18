namespace PMS.Application.Models.AppraisalSettings;

public class AppraisalPeriodDto
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreatePeriodRequest
{
    public string Name { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsActive { get; set; } = true;
}

public class UpdatePeriodRequest
{
    public string Name { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsActive { get; set; }
}

public class PeriodListResponse
{
    public List<AppraisalPeriodDto> Periods { get; set; } = new();
    public PaginationInfo Pagination { get; set; } = new();
}

public class PeriodFilters
{
    public string Search { get; set; } = string.Empty;
    public bool? IsActive { get; set; }
    public int? Year { get; set; }
}
