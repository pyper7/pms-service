namespace PMS.Application.Models.Auth;

public class LoginResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public LoginData? Data { get; set; }
}

public class LoginData
{
    public UserDto User { get; set; } = null!;
    public string Token { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public int ExpiresIn { get; set; }
    public string? Role { get; set; }
}

public class UserDto
{
    public long Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Post { get; set; } = string.Empty;
    public string? StaffId { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Designation { get; set; }
    public List<string> Posts { get; set; } = new();
    public string? OrgUnit { get; set; }
    public string? Cadre { get; set; }
    public string? GradeLevel { get; set; }
    public DateTime? LastLoginDate { get; set; }
}
