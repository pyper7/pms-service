using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PMS.Application.Interfaces;
using PMS.Application.Models.OrganizationalUnits;
using System.Security.Claims;
using PMS.Application.Models.Posts;

namespace PMS.API.Controllers;

[ApiController]
[Route("api/v1/organizational-units")]
[Authorize]
public class OrganizationalUnitsController : ControllerBase
{
    private readonly IOrgUnitService _orgUnitService;
    private readonly IPostService _postService;

    public OrganizationalUnitsController(IOrgUnitService orgUnitService, IPostService postService)
    {
        _orgUnitService = orgUnitService;
        _postService = postService;
    }

    // 1. Organizational Unit Management APIs

    /// <summary>
    /// Get all organizational units with optional filtering and pagination
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        [FromQuery] string? search = null,
        [FromQuery] string? type = null,
        [FromQuery] long? parentId = null,
        [FromQuery] string? status = null,
        [FromQuery] int? level = null,
        [FromQuery] string? sortBy = "name",
        [FromQuery] string? sortOrder = "asc")
    {
        var result = await _orgUnitService.GetAllAsync(page, pageSize, search, type, parentId, status, level, sortBy, sortOrder);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Get posts under a specific organizational unit (department/division/branch)
    /// </summary>
    [HttpGet("{id:long}/posts")]
    [AllowAnonymous]
    public async Task<IActionResult> GetPostsForOrgUnit(
        long id,
        [FromQuery] string? q = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] bool onlyUnassigned = true)
    {
        // Filter by orgUnitId and, by default, only unassigned posts
        bool? isOccupied = onlyUnassigned ? false : (bool?)null;
        var result = await _postService.GetAllAsync(page, pageSize, q, id, null, isOccupied, null, "name", "asc");
        return Ok(result);
    }

    /// <summary>
    /// Get a specific organizational unit by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(long id)
    {
        var result = await _orgUnitService.GetByIdAsync(id);
        return result.Success ? Ok(result) : NotFound(result);
    }

    /// <summary>
    /// Create a new organizational unit
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateOrgUnitRequest request)
    {
        var createdBy = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "system";
        var result = await _orgUnitService.CreateAsync(request, createdBy);
        return result.Success ? CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result) : BadRequest(result);
    }

    /// <summary>
    /// Update an existing organizational unit
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(long id, [FromBody] UpdateOrgUnitRequest request)
    {
        var updatedBy = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "system";
        var result = await _orgUnitService.UpdateAsync(id, request, updatedBy);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Delete an organizational unit and all its children
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(long id, [FromQuery] bool force = false)
    {
        var deletedBy = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "system";
        var result = await _orgUnitService.DeleteAsync(id, force, deletedBy);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    // 2. Organizational Hierarchy APIs

    /// <summary>
    /// Get the complete organizational hierarchy as a tree structure
    /// </summary>
    [HttpGet("tree")]
    public async Task<IActionResult> GetTree(
        [FromQuery] bool includeInactive = false,
        [FromQuery] int? maxDepth = null)
    {
        var result = await _orgUnitService.GetTreeAsync(includeInactive, maxDepth);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Get direct children of a specific organizational unit
    /// </summary>
    [HttpGet("{id}/children")]
    public async Task<IActionResult> GetChildren(long id)
    {
        var result = await _orgUnitService.GetChildrenAsync(id);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Move an organizational unit to a different parent
    /// </summary>
    [HttpPut("{id}/move")]
    public async Task<IActionResult> Move(long id, [FromBody] MoveOrgUnitRequest request)
    {
        var movedBy = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "system";
        var result = await _orgUnitService.MoveAsync(id, request, movedBy);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    // 3. Head of Unit Management APIs

    /// <summary>
    /// Assign a staff member as head of an organizational unit
    /// </summary>
    [HttpPost("{id}/head")]
    public async Task<IActionResult> AssignHead(long id, [FromBody] AssignHeadRequest request)
    {
        var assignedBy = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "system";
        var result = await _orgUnitService.AssignHeadAsync(id, request, assignedBy);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Remove the current head of an organizational unit
    /// </summary>
    [HttpDelete("{id}/head")]
    public async Task<IActionResult> RemoveHead(long id, [FromBody] RemoveHeadRequest request)
    {
        var removedBy = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "system";
        var result = await _orgUnitService.RemoveHeadAsync(id, request, removedBy);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Get the history of heads of an organizational unit
    /// </summary>
    [HttpGet("{id}/head/history")]
    public async Task<IActionResult> GetHeadHistory(long id)
    {
        var result = await _orgUnitService.GetHeadHistoryAsync(id);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    // 4. Organizational Unit Statistics APIs

    /// <summary>
    /// Get comprehensive statistics about organizational units
    /// </summary>
    [HttpGet("statistics")]
    public async Task<IActionResult> GetStatistics(
        [FromQuery] string? type = null)
    {
        var result = await _orgUnitService.GetStatisticsAsync(type);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Get performance metrics for a specific organizational unit
    /// </summary>
    [HttpGet("{id}/metrics")]
    public async Task<IActionResult> GetMetrics(long id)
    {
        var result = await _orgUnitService.GetMetricsAsync(id);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    // 5. Bulk Operations APIs

    /// <summary>
    /// Create multiple organizational units in a single operation
    /// </summary>
    [HttpPost("bulk")]
    public async Task<IActionResult> BulkCreate([FromBody] BulkCreateOrgUnitsRequest request)
    {
        var createdBy = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "system";
        var result = await _orgUnitService.BulkCreateAsync(request, createdBy);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Update multiple organizational units in a single operation
    /// </summary>
    [HttpPut("bulk")]
    public async Task<IActionResult> BulkUpdate([FromBody] BulkUpdateOrgUnitsRequest request)
    {
        var updatedBy = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "system";
        var result = await _orgUnitService.BulkUpdateAsync(request, updatedBy);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    // 6. Export APIs

    /// <summary>
    /// Export organizational units data in various formats
    /// </summary>
    [HttpGet("export")]
    public async Task<IActionResult> Export(
        [FromQuery] string format = "csv",
        [FromQuery] string? type = null,
        [FromQuery] bool includeInactive = false)
    {
        return await _orgUnitService.ExportAsync(format, type, includeInactive);
    }

    /// <summary>
    /// Export the organizational hierarchy as a visual diagram
    /// </summary>
    [HttpGet("tree/export")]
    public async Task<IActionResult> ExportTree(
        [FromQuery] string format = "png",
        [FromQuery] int? maxDepth = null)
    {
        return await _orgUnitService.ExportTreeAsync(format, maxDepth);
    }

    // 7. Validation APIs

    /// <summary>
    /// Validate organizational unit data before creation or update
    /// </summary>
    [HttpPost("validate")]
    public async Task<IActionResult> Validate([FromBody] ValidateOrgUnitRequest request)
    {
        var result = await _orgUnitService.ValidateAsync(request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Check if an organizational unit name is available
    /// </summary>
    [HttpGet("check-name")]
    public async Task<IActionResult> CheckNameAvailability(
        [FromQuery] string name,
        [FromQuery] long? parentId = null,
        [FromQuery] long? excludeId = null)
    {
        var result = await _orgUnitService.CheckNameAvailabilityAsync(name, parentId, excludeId);
        return result.Success ? Ok(result) : BadRequest(result);
    }
}
