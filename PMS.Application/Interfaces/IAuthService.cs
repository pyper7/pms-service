using PMS.Application.Models.Auth;
using PMS.Application.Models;
using PMS.Domain.Entities;

namespace PMS.Application.Interfaces;

public interface IAuthService
{
    Task<ApiResponse<LoginData>> LoginAsync(LoginRequest request);
    Task<RefreshTokenResponse> RefreshTokenAsync(RefreshTokenRequest request);
    Task<ApiResponse<UserDto>> GetProfileAsync(long userId);
    Task<List<string>> GetPermissionNamesAsync(long userId);
    Task<bool> LogoutAsync(long userId);
    Task<bool> ValidateUserAsync(string emailOrStaffId, string password);
    Task<User?> GetUserByEmailAsync(string email);
    Task<User?> GetUserByStaffIdAsync(string staffId);
}
