namespace PMS.Application.Models.AppraisalSettings;

public class PaginationInfo
{
    public int CurrentPage { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
    public int TotalItems { get; set; }
    public bool HasNext { get; set; }
    public bool HasPrevious { get; set; }
}

public class ToggleStatusResponse
{
    public long Id { get; set; }
    public bool IsActive { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class ExportRequest
{
    public string Format { get; set; } = "json"; // json, csv, excel
    public string? Category { get; set; }
    public bool? IsActive { get; set; }
}

public class ImportResponse
{
    public int Imported { get; set; }
    public int Skipped { get; set; }
    public int Errors { get; set; }
    public List<BulkError> Details { get; set; } = new();
}
