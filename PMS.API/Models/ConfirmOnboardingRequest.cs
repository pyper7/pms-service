using System.ComponentModel.DataAnnotations;

namespace PMS.API.Models;

public class ConfirmOnboardingRequest
{
    [Required]
    [StringLength(50)]
    public string StaffId { get; set; } = string.Empty;

    [Required]
    public long DepartmentId { get; set; }

    public long? DivisionId { get; set; }

    public long? BranchId { get; set; }

    [Required]
    public long PostId { get; set; }

    [Required]
    [StringLength(100)]
    public string ReservationId { get; set; } = string.Empty;
}


