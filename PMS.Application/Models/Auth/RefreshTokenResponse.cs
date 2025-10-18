namespace PMS.Application.Models.Auth;

public class RefreshTokenResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public RefreshTokenData? Data { get; set; }
}

public class RefreshTokenData
{
    public string Token { get; set; } = string.Empty;
    public int ExpiresIn { get; set; }
}
