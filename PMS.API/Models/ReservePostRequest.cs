using System.ComponentModel.DataAnnotations;

namespace PMS.API.Models;

public class ReservePostRequest
{
    [Required]
    [StringLength(50)]
    public string StaffId { get; set; } = string.Empty;

    [Required]
    public long PostId { get; set; }

    public long? DepartmentId { get; set; }
    public long? DivisionId { get; set; }
    public long? BranchId { get; set; }
}


