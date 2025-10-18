using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PMS.Application.Interfaces;
using PMS.Application.Models;
using PMS.Application.Models.Posts;
using PMS.API.Models;

namespace PMS.API.Controllers;

[ApiController]
[Route("api/v1/posts")]
[Authorize]
public class PostsController : ControllerBase
{
    private readonly IPostService _postService;
    private readonly IUserRepository _userRepository;
    private readonly IReservationService _reservationService;

    public PostsController(IPostService postService, IUserRepository userRepository, IReservationService reservationService)
    {
        _postService = postService;
        _userRepository = userRepository;
        _reservationService = reservationService;
    }

    /// <summary>
    /// Get all posts with pagination and filtering
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<PostListDto>>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int limit = 10,
        [FromQuery] string? search = null,
        [FromQuery] long? orgUnitId = null,
        [FromQuery] string? status = null,
        [FromQuery] bool? isOccupied = null,
        [FromQuery] string? gradeLevel = null,
        [FromQuery] string? sortBy = "name",
        [FromQuery] string? sortOrder = "asc")
    {
        var result = await _postService.GetAllAsync(page, limit, search, orgUnitId, status, isOccupied, gradeLevel, sortBy, sortOrder);
        return Ok(result);
    }

    /// <summary>
    /// Get posts by scope (department/division/branch)
    /// </summary>
    [HttpGet("by-scope")] 
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<object>>> GetByScope([FromQuery] long? departmentId, [FromQuery] long? divisionId, [FromQuery] long? branchId, [FromQuery] string? q = null, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        // Use existing GetAll with orgUnitId filter. Assuming: department/division/branch are OrgUnits at different levels
        long? orgUnitId = branchId ?? divisionId ?? departmentId;
        // Fetch only unassigned posts (isOccupied = false)
        var result = await _postService.GetAllAsync(page, pageSize, q, orgUnitId, null, false, null, "name", "asc");
        return Ok(result);
    }

    /// <summary>
    /// Check and reserve post for onboarding (short-lived)
    /// </summary>
    [HttpPost("check-and-reserve")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<object>>> CheckAndReserve([FromBody] ReservePostRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<object>.Fail("Invalid request data", "VALIDATION_ERROR", ModelState));

        var user = await _userRepository.GetByStaffIdAsync(request.StaffId);
        if (user == null)
            return NotFound(ApiResponse<object>.Fail("Invalid staff ID", "INVALID_STAFF_ID"));

        if (user.IsActive && user.IsPostAssigned)
            return Conflict(ApiResponse<object>.Fail("User already onboarded", "ONBOARDING_COMPLETED"));

        var idempotencyKey = Request.Headers["Idempotency-Key"].FirstOrDefault() ?? Guid.NewGuid().ToString();
        var rv = await _reservationService.ReservePostAsync(request.StaffId, request.PostId, request.DepartmentId, request.DivisionId, request.BranchId, idempotencyKey);
        if (!rv.Success)
            return BadRequest(ApiResponse<object>.Fail(rv.Error!, rv.ErrorCode!));

        return Ok(ApiResponse<object>.Ok(new { reservationId = rv.ReservationId, expiresAt = rv.ExpiresAt }, "Post reserved"));
    }

    /// <summary>
    /// Get post by ID
    /// </summary>
    [HttpGet("{id:long}")]
    public async Task<ActionResult<ApiResponse<PostDto>>> GetById(long id)
    {
        var result = await _postService.GetByIdAsync(id);
        if (!result.Success) return NotFound(result);
        return Ok(result);
    }

    /// <summary>
    /// Create new post
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ApiResponse<PostDto>>> Create([FromBody] CreatePostRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<PostDto>.Fail("Validation failed", "VALIDATION_ERROR", ModelState));

        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "system";
        var result = await _postService.CreateAsync(request, userId);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    /// <summary>
    /// Update post
    /// </summary>
    [HttpPut("{id:long}")]
    public async Task<ActionResult<ApiResponse<PostDto>>> Update(long id, [FromBody] UpdatePostRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<PostDto>.Fail("Validation failed", "VALIDATION_ERROR", ModelState));

        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "system";
        var result = await _postService.UpdateAsync(id, request, userId);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    /// <summary>
    /// Delete post
    /// </summary>
    [HttpDelete("{id:long}")]
    public async Task<ActionResult<ApiResponse<object>>> Delete(long id)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "system";
        var result = await _postService.DeleteAsync(id, userId);
        if (!result.Success) return NotFound(result);
        return Ok(result);
    }

    /// <summary>
    /// Assign role to post
    /// </summary>
    [HttpPost("{id:long}/role")]
    public async Task<ActionResult<ApiResponse<object>>> AssignRole(long id, [FromBody] AssignRoleToPostRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<object>.Fail("Validation failed", "VALIDATION_ERROR", ModelState));

        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "system";
        var result = await _postService.AssignRoleAsync(id, request.RoleId, userId);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    /// <summary>
    /// Remove role from post
    /// </summary>
    [HttpDelete("{id:long}/role")]
    public async Task<ActionResult<ApiResponse<object>>> RemoveRole(long id)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "system";
        var result = await _postService.RemoveRoleAsync(id, userId);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    /// <summary>
    /// Get post occupancy history
    /// </summary>
    [HttpGet("{id:long}/occupancy-history")]
    public async Task<ActionResult<ApiResponse<PostOccupancyListDto>>> GetOccupancyHistory(
        long id,
        [FromQuery] int page = 1,
        [FromQuery] int limit = 10,
        [FromQuery] bool includeInactive = true)
    {
        var result = await _postService.GetOccupancyHistoryAsync(id, page, limit, includeInactive);
        if (!result.Success) return NotFound(result);
        return Ok(result);
    }

    /// <summary>
    /// Validate post assignment
    /// </summary>
    [HttpPost("{id:long}/validate-assignment")]
    public async Task<ActionResult<ApiResponse<AssignmentValidationDto>>> ValidateAssignment(
        long id, 
        [FromBody] ValidateAssignmentRequest request)
    {
        var result = await _postService.ValidateAssignmentAsync(id, request.OfficerId);
        return Ok(result);
    }

    /// <summary>
    /// Check post availability
    /// </summary>
    [HttpGet("{id:long}/availability")]
    public async Task<ActionResult<ApiResponse<PostAvailabilityDto>>> CheckAvailability(long id)
    {
        var result = await _postService.CheckAvailabilityAsync(id);
        return Ok(result);
    }

    /// <summary>
    /// Get post statistics
    /// </summary>
    [HttpGet("statistics")]
    public async Task<ActionResult<ApiResponse<PostStatisticsDto>>> GetStatistics()
    {
        var result = await _postService.GetStatisticsAsync();
        return Ok(result);
    }

    /// <summary>
    /// Export posts
    /// </summary>
    [HttpGet("export")]
    public async Task<IActionResult> Export(
        [FromQuery] string format = "csv",
        [FromQuery] string? search = null,
        [FromQuery] long? orgUnitId = null,
        [FromQuery] string? status = null,
        [FromQuery] bool? isOccupied = null,
        [FromQuery] string? gradeLevel = null)
    {
        var result = await _postService.ExportAsync(format, search, orgUnitId, status, isOccupied, gradeLevel);
        return result;
    }

    /// <summary>
    /// Bulk create posts
    /// </summary>
    [HttpPost("bulk")]
    public async Task<ActionResult<ApiResponse<object>>> BulkCreate([FromBody] BulkCreatePostsRequest request)
    {
        var result = await _postService.BulkCreateAsync(request.Posts);
        return Ok(result);
    }
}
