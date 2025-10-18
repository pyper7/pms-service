using System.ComponentModel.DataAnnotations;

namespace PMS.Application.Models.Auth;

public class RefreshTokenRequest
{
    [Required]
    public string RefreshToken { get; set; } = string.Empty;
}
