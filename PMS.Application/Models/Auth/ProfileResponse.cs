namespace PMS.Application.Models.Auth;

public class ProfileResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public UserDto? Data { get; set; }
}
