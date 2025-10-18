using BCrypt.Net;
using PMS.Application.Interfaces;
using PMS.Application.Models;
using PMS.Application.Models.Auth;
using PMS.Domain.Entities;

namespace PMS.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IJwtService _jwtService;
    private readonly Dictionary<string, string> _refreshTokens = new(); // In production, use Redis or database

    public AuthService(IUserRepository userRepository, IJwtService jwtService)
    {
        _userRepository = userRepository;
        _jwtService = jwtService;
    }

    public async Task<ApiResponse<LoginData>> LoginAsync(LoginRequest request)
    {
        try
        {
            // Try to determine if the input is an email or StaffId
            User? user = null;
            if (request.EmailOrStaffId.Contains("@"))
            {
                // It's an email
                user = await _userRepository.GetByEmailAsync(request.EmailOrStaffId);
            }
            else
            {
                // It's a StaffId
                user = await _userRepository.GetByStaffIdAsync(request.EmailOrStaffId);
            }

            if (user == null)
            {
                return ApiResponse<LoginData>.Fail("Invalid email/StaffId or password");
            }

            // Check if user has not completed onboarding
            if (!user.IsActive && !user.IsPostAssigned)
            {
                return ApiResponse<LoginData>.Fail("User has not completed onboarding. Please complete the registration process first.", errorCode: "ONBOARDING_INCOMPLETE");
            }

            if (!user.IsActive)
            {
                return ApiResponse<LoginData>.Fail("Invalid email/StaffId or password");
            }

            if (user.PasswordHash == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            {
                return ApiResponse<LoginData>.Fail("Invalid email/StaffId or password");
            }

            // Update last login
            user.LastLoginDate = DateTime.UtcNow;
            await _userRepository.UpdateAsync(user);

            // Get user posts and roles from current assignments
            var userWithPosts = await _userRepository.GetWithPostsAsync(user.Id);
            var posts = userWithPosts?.PostOccupancies
                .Where(po => po.IsCurrentlyOccupying)
                .Select(po => po.Post?.Title)
                .Where(postTitle => !string.IsNullOrEmpty(postTitle))
                .Distinct()
                .ToList() ?? new List<string>();

            var role = userWithPosts?.PostOccupancies
                .Where(po => po.IsCurrentlyOccupying)
                .Select(po => po.Post?.Role?.Name)
                .FirstOrDefault(name => !string.IsNullOrEmpty(name));

            // Generate tokens
            var token = _jwtService.GenerateToken(user, posts);
            var refreshToken = _jwtService.GenerateRefreshToken();

            // Store refresh token (in production, store in database)
            _refreshTokens[refreshToken] = user.Id.ToString();

            return ApiResponse<LoginData>.Ok(new LoginData
            {
                User = _jwtService.CreateUserDto(user),
                Token = token,
                RefreshToken = refreshToken,
                ExpiresIn = 86400,
                Role = role
            }, "Login successful");
        }
        catch (Exception ex)
        {
            return ApiResponse<LoginData>.Fail($"Login failed: {ex.Message}", errorCode: "LOGIN_FAILED");
        }
    }

    public async Task<RefreshTokenResponse> RefreshTokenAsync(RefreshTokenRequest request)
    {
        try
        {
            if (!_jwtService.ValidateRefreshToken(request.RefreshToken))
            {
                return new RefreshTokenResponse
                {
                    Success = false,
                    Message = "Invalid refresh token"
                };
            }

            if (!_refreshTokens.TryGetValue(request.RefreshToken, out var userIdStr) || 
                !long.TryParse(userIdStr, out var userId))
            {
                return new RefreshTokenResponse
                {
                    Success = false,
                    Message = "Invalid refresh token"
                };
            }

            var user = await _userRepository.GetWithPostsAsync(userId);
            if (user == null || !user.IsActive)
            {
                return new RefreshTokenResponse
                {
                    Success = false,
                    Message = "User not found or inactive"
                };
            }

            var posts = user.PostOccupancies
                .Where(po => po.IsCurrentlyOccupying)
                .Select(po => po.Post?.Title)
                .Where(postTitle => !string.IsNullOrEmpty(postTitle))
                .Distinct()
                .ToList()!;

            var token = _jwtService.GenerateToken(user, posts);
            var newRefreshToken = _jwtService.GenerateRefreshToken();

            // Remove old refresh token and add new one
            _refreshTokens.Remove(request.RefreshToken);
            _refreshTokens[newRefreshToken] = userId.ToString();

            return new RefreshTokenResponse
            {
                Success = true,
                Message = "Token refreshed successfully",
                Data = new RefreshTokenData
                {
                    Token = token,
                    ExpiresIn = 86400
                }
            };
        }
        catch (Exception ex)
        {
            return new RefreshTokenResponse
            {
                Success = false,
                Message = $"Token refresh failed: {ex.Message}"
            };
        }
    }

    public async Task<ApiResponse<UserDto>> GetProfileAsync(long userId)
    {
        try
        {
            var user = await _userRepository.GetWithPostsAsync(userId);
            if (user == null)
            {
                return ApiResponse<UserDto>.Fail("User not found", errorCode: "USER_NOT_FOUND");
            }

            return ApiResponse<UserDto>.Ok(_jwtService.CreateUserDto(user), "Profile retrieved successfully");
        }
        catch (Exception ex)
        {
            return ApiResponse<UserDto>.Fail($"Failed to retrieve profile: {ex.Message}", errorCode: "PROFILE_FAILED");
        }
    }

    public Task<bool> LogoutAsync(long userId)
    {
        try
        {
            // Remove all refresh tokens for this user
            var tokensToRemove = _refreshTokens
                .Where(kvp => kvp.Value == userId.ToString())
                .Select(kvp => kvp.Key)
                .ToList();

            foreach (var token in tokensToRemove)
            {
                _refreshTokens.Remove(token);
            }

            return Task.FromResult(true);
        }
        catch
        {
            return Task.FromResult(false);
        }
    }

    public async Task<List<string>> GetPermissionNamesAsync(long userId)
    {
            var user = await _userRepository.GetWithPostsAsync(userId);
            if (user == null || !user.IsActive)
            {
                return new List<string>();
            }

        return await (_userRepository.GetPermissionNamesAsync(userId));
    }

    public async Task<bool> ValidateUserAsync(string emailOrStaffId, string password)
    {
        User? user = null;
        if (emailOrStaffId.Contains("@"))
        {
            user = await _userRepository.GetByEmailAsync(emailOrStaffId);
        }
        else
        {
            user = await _userRepository.GetByStaffIdAsync(emailOrStaffId);
        }

        if (user == null || !user.IsActive || user.PasswordHash == null)
            return false;

        return BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);
    }

    public async Task<User?> GetUserByEmailAsync(string email)
    {
        return await _userRepository.GetByEmailAsync(email);
    }

    public async Task<User?> GetUserByStaffIdAsync(string staffId)
    {
        return await _userRepository.GetByStaffIdAsync(staffId);
    }
}
