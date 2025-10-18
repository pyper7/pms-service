using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PMS.Application.Interfaces;
using PMS.Application.Models;
using PMS.Application.Models.Posts;

namespace PMS.API.Controllers;

[ApiController]
[Route("api/v1/post-occupancies")]
[Authorize]
public class PostOccupanciesController : ControllerBase
{
    private readonly IPostService _postService;

    public PostOccupanciesController(IPostService postService)
    {
        _postService = postService;
    }

    /// <summary>
    /// Get all post occupancies with pagination and filtering
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<PostOccupancyListDto>>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int limit = 10,
        [FromQuery] string? search = null,
        [FromQuery] long? postId = null,
        [FromQuery] long? officerId = null,
        [FromQuery] bool? isActive = null,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] string? sortBy = "startDate",
        [FromQuery] string? sortOrder = "desc")
    {
        var result = await _postService.GetAllOccupanciesAsync(page, limit, search, postId, officerId, isActive, startDate, endDate, sortBy, sortOrder);
        return Ok(result);
    }

    /// <summary>
    /// Get single post occupancy by ID
    /// </summary>
    [HttpGet("{id:long}")]
    public async Task<ActionResult<ApiResponse<PostOccupancyDto>>> GetById(long id)
    {
        var result = await _postService.GetOccupancyByIdAsync(id);
        if (!result.Success) return NotFound(result);
        return Ok(result);
    }

    /// <summary>
    /// Assign officer to post
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ApiResponse<PostOccupancyDto>>> Create([FromBody] CreatePostOccupancyRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<PostOccupancyDto>.Fail("Validation failed", "VALIDATION_ERROR", ModelState));

        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "system";
        var result = await _postService.CreateOccupancyAsync(request, userId);
        if (!result.Success) return BadRequest(result);
        return CreatedAtAction(nameof(GetById), new { id = result.Data?.Id }, result);
    }

    /// <summary>
    /// Update post occupancy
    /// </summary>
    [HttpPut("{id:long}")]
    public async Task<ActionResult<ApiResponse<PostOccupancyDto>>> Update(long id, [FromBody] UpdatePostOccupancyRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<PostOccupancyDto>.Fail("Validation failed", "VALIDATION_ERROR", ModelState));

        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "system";
        var result = await _postService.UpdateOccupancyAsync(id, request, userId);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    /// <summary>
    /// End post occupancy
    /// </summary>
    [HttpPut("{id:long}/end")]
    public async Task<ActionResult<ApiResponse<PostOccupancyDto>>> EndOccupancy(long id, [FromBody] EndPostOccupancyRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<PostOccupancyDto>.Fail("Validation failed", "VALIDATION_ERROR", ModelState));

        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "system";
        var result = await _postService.EndOccupancyAsync(id, request, userId);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    /// <summary>
    /// Delete post occupancy
    /// </summary>
    [HttpDelete("{id:long}")]
    public async Task<ActionResult<ApiResponse<object>>> Delete(long id)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "system";
        var result = await _postService.DeleteOccupancyAsync(id, userId);
        if (!result.Success) return NotFound(result);
        return Ok(result);
    }

    /// <summary>
    /// Get occupancy statistics
    /// </summary>
    [HttpGet("statistics")]
    public async Task<ActionResult<ApiResponse<OccupancyStatisticsDto>>> GetStatistics()
    {
        var result = await _postService.GetOccupancyStatisticsAsync();
        return Ok(result);
    }

    /// <summary>
    /// Export post occupancies
    /// </summary>
    [HttpGet("export")]
    public async Task<IActionResult> Export(
        [FromQuery] string format = "csv",
        [FromQuery] string? search = null,
        [FromQuery] long? postId = null,
        [FromQuery] long? officerId = null,
        [FromQuery] bool? isActive = null,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        var result = await _postService.ExportOccupanciesAsync(format, search, postId, officerId, isActive, startDate, endDate);
        return result;
    }

    /// <summary>
    /// Bulk assign officers
    /// </summary>
    [HttpPost("bulk")]
    public async Task<ActionResult<ApiResponse<object>>> BulkAssign([FromBody] BulkAssignOfficersRequest request)
    {
        var result = await _postService.BulkAssignOfficersAsync(request.Assignments);
        return Ok(result);
    }
}
