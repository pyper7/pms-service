using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PMS.Application.Interfaces;
using PMS.Application.Models;
using PMS.Application.Models.Auth;
using PMS.API.Models;
using BCrypt.Net;
using PMS.Application.Models.Posts;

namespace PMS.API.Controllers;

[ApiController]
[Route("api/v1/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IUserRepository _userRepository;
    private readonly IReservationService _reservationService;
    private readonly IPostService _postService;

    public AuthController(IAuthService authService, IUserRepository userRepository, IReservationService reservationService, IPostService postService)
    {
        _authService = authService;
        _userRepository = userRepository;
        _reservationService = reservationService;
        _postService = postService;
    }

    /// <summary>
    /// Authenticate user and return JWT token
    /// </summary>
    [HttpPost("login")]
    public async Task<ActionResult<ApiResponse<LoginData>>> Login([FromBody] LoginRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<LoginData>.Fail("Invalid request data", errorCode: "VALIDATION_ERROR"));
        }

        var result = await _authService.LoginAsync(request);
        
        if (!result.Success)
        {
            return Unauthorized(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Set initial password after staffId validation
    /// </summary>
    [HttpPost("set-password")]
    public async Task<ActionResult<ApiResponse<object>>> SetPassword([FromBody] SetPasswordRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<object>.Fail("Invalid request data", "VALIDATION_ERROR", ModelState));

        var user = await _userRepository.GetByStaffIdAsync(request.StaffId);
        if (user == null)
            return NotFound(ApiResponse<object>.Fail("Invalid staff ID", "INVALID_STAFF_ID"));

        if (user.IsActive && user.IsPostAssigned)
            return BadRequest(ApiResponse<object>.Fail("User has already completed onboarding", "ONBOARDING_COMPLETED"));

        // Set password hash
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
        await _userRepository.UpdateAsync(user);
        return Ok(ApiResponse<object>.Ok(null, "Password set successfully"));
    }

    /// <summary>
    /// Confirm onboarding (assigns post and activates user)
    /// </summary>
    [HttpPost("onboard/confirm")]
    public async Task<ActionResult<ApiResponse<object>>> ConfirmOnboarding([FromBody] ConfirmOnboardingRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<object>.Fail("Invalid request data", "VALIDATION_ERROR", ModelState));

        var user = await _userRepository.GetByStaffIdAsync(request.StaffId);
        if (user == null)
            return NotFound(ApiResponse<object>.Fail("Invalid staff ID", "INVALID_STAFF_ID"));

        if (user.IsActive && user.IsPostAssigned)
            return Conflict(ApiResponse<object>.Fail("User already onboarded", "ONBOARDING_COMPLETED"));

        // Validate reservation
        var rv = await _reservationService.ValidateReservationAsync(request.ReservationId, request.StaffId, request.PostId);
        if (!rv.Success)
            return BadRequest(ApiResponse<object>.Fail(rv.Error!, rv.ErrorCode!));

        // Create post occupancy for the user
        var occupancyRequest = new CreatePostOccupancyRequest
        {
            PostId = request.PostId,
            OfficerId = user.Id,
            Notes = "Onboarding assignment",
            StartDate = DateTime.UtcNow
        };

        // createdBy as "system" since onboarding is unauthenticated
        var occupancyResult = await _postService.CreateOccupancyAsync(occupancyRequest, "system");
        if (!occupancyResult.Success)
            return BadRequest(ApiResponse<object>.Fail(occupancyResult.Message ?? "Failed to create occupancy", occupancyResult.ErrorCode ?? "OCCUPANCY_FAILED"));

        // Flip flags after successful occupancy creation
        user.IsActive = true;
        user.IsPostAssigned = true;
        await _userRepository.UpdateAsync(user);

        // Invalidate reservation
        await _reservationService.InvalidateReservationAsync(request.ReservationId);

        return Ok(ApiResponse<object>.Ok(new {
            userId = user.Id,
            assignedPost = new { id = request.PostId },
            occupancyId = occupancyResult.Data?.Id
        }, "Onboarding completed"));
    }

    /// <summary>
    /// Refresh JWT token using refresh token
    /// </summary>
    [HttpPost("refresh")]
    public async Task<ActionResult<RefreshTokenResponse>> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new RefreshTokenResponse
            {
                Success = false,
                Message = "Invalid request data"
            });
        }

        var result = await _authService.RefreshTokenAsync(request);
        
        if (!result.Success)
        {
            return Unauthorized(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Get current user profile
    /// </summary>
    [HttpGet("profile")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<UserDto>>> GetProfile()
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
            ?? User.FindFirst("nameid")?.Value;
        if (userIdClaim == null || !long.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(ApiResponse<UserDto>.Fail("Invalid user token"));
        }

        var result = await _authService.GetProfileAsync(userId);
        
        if (!result.Success)
        {
            return NotFound(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Logout user and invalidate refresh token
    /// </summary>
    [HttpPost("logout")]
   // [Authorize]
    public async Task<ActionResult> Logout()
    {
        var userIdClaim = User.FindFirst("nameid")?.Value;
        if (userIdClaim == null || !long.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(new { message = "Invalid user token" });
        }

        var success = await _authService.LogoutAsync(userId);
        
        if (!success)
        {
            return BadRequest(new { message = "Logout failed" });
        }

        return Ok(new { message = "Logged out successfully" });
    }

    /// <summary>
    /// Get current user's permission names
    /// </summary>
    [HttpGet("permissions")]
    [Authorize]
    public async Task<ActionResult<IEnumerable<string>>> GetPermissions()
    {
        var userIdClaim = User.FindFirst("nameid")?.Value;
        if (userIdClaim == null || !long.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(ApiResponse.Fail("Invalid user token"));
        }

        var permissions = await _authService.GetPermissionNamesAsync(userId);
        return Ok(ApiResponse<object>.Ok(permissions));
    }

    /// <summary>
    /// Validate staff ID for new user registration
    /// </summary>
    [HttpPost("validate-staff-id")]
    public async Task<ActionResult<ApiResponse<object>>> ValidateStaffId([FromBody] ValidateStaffIdRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<object>.Fail("Invalid request data", errorCode: "VALIDATION_ERROR"));
        }

        if (string.IsNullOrWhiteSpace(request.StaffId))
        {
            return BadRequest(ApiResponse<object>.Fail("Staff ID is required", errorCode: "STAFF_ID_REQUIRED"));
        }

        try
        {
            var user = await _userRepository.GetByStaffIdAsync(request.StaffId.Trim());

            // Staff ID doesn't exist
            if (user == null)
            {
                return NotFound(ApiResponse<object>.Fail("Invalid staff ID", errorCode: "INVALID_STAFF_ID"));
            }

            // Staff ID exists but user has completed onboarding
            if (user.IsActive && user.IsPostAssigned)
            {
                return BadRequest(ApiResponse<object>.Fail("User has already completed onboarding", errorCode: "ONBOARDING_COMPLETED"));
            }

            // Staff ID exists and user hasn't completed onboarding (IsActive = false and IsPostAssigned = false)
            if (!user.IsActive && !user.IsPostAssigned)
            {
                return Ok(ApiResponse<object>.Ok(new { 
                    StaffId = user.StaffId,
                    Name = user.FullName,
                    Email = user.Email,
                    Department = user.Department,
                    Position = user.Position
                }, "Staff ID is valid for registration"));
            }

            // Any other combination is not valid for new registration
            return BadRequest(ApiResponse<object>.Fail("Staff ID is not available for new registration", errorCode: "STAFF_ID_NOT_AVAILABLE"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<object>.Fail($"An error occurred while validating staff ID: {ex.Message}", errorCode: "VALIDATION_ERROR"));
        }
    }
}
