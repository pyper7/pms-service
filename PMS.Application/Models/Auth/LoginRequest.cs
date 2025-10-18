using System.ComponentModel.DataAnnotations;

namespace PMS.Application.Models.Auth;

public class LoginRequest
{
    [Required]
    public string EmailOrStaffId { get; set; } = string.Empty;
    
    [Required]
    [MinLength(6)]
    public string Password { get; set; } = string.Empty;
}
