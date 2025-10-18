namespace PMS.Application.Models.Permissions;

public class PermissionDto
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Module { get; set; }
}

public class PermissionListResponse
{
    public List<PermissionDto> Permissions { get; set; } = new();
    public Dictionary<string, List<PermissionDto>> PermissionsByModule { get; set; } = new();
    public int TotalCount { get; set; }
}
