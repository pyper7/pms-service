using System.ComponentModel.DataAnnotations;

namespace PMS.API.Models;

public class ValidateStaffIdRequest
{
    [Required(ErrorMessage = "Staff ID is required")]
    [StringLength(50, ErrorMessage = "Staff ID cannot exceed 50 characters")]
    public string StaffId { get; set; } = string.Empty;
}
