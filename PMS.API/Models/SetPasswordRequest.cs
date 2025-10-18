using System.ComponentModel.DataAnnotations;

namespace PMS.API.Models;

public class SetPasswordRequest
{
    [Required]
    [StringLength(50)]
    public string StaffId { get; set; } = string.Empty;

    [Required]
    [StringLength(100, MinimumLength = 8)]
    [RegularExpression("^(?=.*[a-z])(?=.*[A-Z])(?=.*\\d).+$", ErrorMessage = "Password must contain lowercase, uppercase, and a number")]
    public string Password { get; set; } = string.Empty;
}


