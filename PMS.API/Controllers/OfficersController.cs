using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PMS.Application.Interfaces;
using PMS.Application.Models;
using PMS.Application.Models.Officers;

namespace PMS.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class OfficersController : ControllerBase
{
    private readonly IOfficerService _officerService;

    public OfficersController(IOfficerService officerService)
    {
        _officerService = officerService;
    }

    /// <summary>
    /// Get all officers with optional filtering and search
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int limit = 10,
        [FromQuery] string? search = null,
        [FromQuery] string? department = null,
        [FromQuery] string? gradeLevel = null,
        [FromQuery] string? status = null,
        [FromQuery] string? position = null,
        [FromQuery] string? sortBy = "name",
        [FromQuery] string? sortOrder = "asc")
    {
        var result = await _officerService.GetAllAsync(page, limit, search, department, gradeLevel, status, position, sortBy, sortOrder);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    /// <summary>
    /// Get officer by ID
    /// </summary>
    [HttpGet("{id:long}")]
    public async Task<IActionResult> GetById(long id)
    {
        var result = await _officerService.GetByIdAsync(id);
        if (!result.Success) return NotFound(result);
        return Ok(result);
    }

    /// <summary>
    /// Create new officer
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateOfficerRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<OfficerDto>.Fail("Validation failed", "VALIDATION_ERROR", ModelState));

        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "system";
        var result = await _officerService.CreateAsync(request, userId);
        if (!result.Success) return BadRequest(result);
        return CreatedAtAction(nameof(GetById), new { id = result.Data?.Id }, result);
    }

    /// <summary>
    /// Update officer
    /// </summary>
    [HttpPut("{id:long}")]
    public async Task<IActionResult> Update(long id, [FromBody] UpdateOfficerRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<OfficerDto>.Fail("Validation failed", "VALIDATION_ERROR", ModelState));

        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "system";
        var result = await _officerService.UpdateAsync(id, request, userId);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    /// <summary>
    /// Delete officer
    /// </summary>
    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Delete(long id)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "system";
        var result = await _officerService.DeleteAsync(id, userId);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    /// <summary>
    /// Get officer qualifications
    /// </summary>
    [HttpGet("{id:long}/qualifications")]
    public async Task<IActionResult> GetQualifications(long id)
    {
        var result = await _officerService.GetQualificationsAsync(id);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    /// <summary>
    /// Add officer qualification
    /// </summary>
    [HttpPost("{id:long}/qualifications")]
    public async Task<IActionResult> AddQualification(long id, [FromBody] CreateQualificationRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<QualificationDto>.Fail("Validation failed", "VALIDATION_ERROR", ModelState));

        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "system";
        var result = await _officerService.AddQualificationAsync(id, request, userId);
        if (!result.Success) return BadRequest(result);
        return CreatedAtAction(nameof(GetQualifications), new { id }, result);
    }

    /// <summary>
    /// Update officer qualification
    /// </summary>
    [HttpPut("{id:long}/qualifications/{qualificationId:long}")]
    public async Task<IActionResult> UpdateQualification(long id, long qualificationId, [FromBody] UpdateQualificationRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<QualificationDto>.Fail("Validation failed", "VALIDATION_ERROR", ModelState));

        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "system";
        var result = await _officerService.UpdateQualificationAsync(id, qualificationId, request, userId);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    /// <summary>
    /// Delete officer qualification
    /// </summary>
    [HttpDelete("{id:long}/qualifications/{qualificationId:long}")]
    public async Task<IActionResult> DeleteQualification(long id, long qualificationId)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "system";
        var result = await _officerService.DeleteQualificationAsync(id, qualificationId, userId);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    /// <summary>
    /// Get officer work history
    /// </summary>
    [HttpGet("{id:long}/work-history")]
    public async Task<IActionResult> GetWorkHistory(long id)
    {
        var result = await _officerService.GetWorkHistoryAsync(id);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    /// <summary>
    /// Add work history entry
    /// </summary>
    [HttpPost("{id:long}/work-history")]
    public async Task<IActionResult> AddWorkHistory(long id, [FromBody] CreateWorkHistoryRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<WorkHistoryDto>.Fail("Validation failed", "VALIDATION_ERROR", ModelState));

        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "system";
        var result = await _officerService.AddWorkHistoryAsync(id, request, userId);
        if (!result.Success) return BadRequest(result);
        return CreatedAtAction(nameof(GetWorkHistory), new { id }, result);
    }

    /// <summary>
    /// Update work history entry
    /// </summary>
    [HttpPut("{id:long}/work-history/{workHistoryId:long}")]
    public async Task<IActionResult> UpdateWorkHistory(long id, long workHistoryId, [FromBody] CreateWorkHistoryRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<WorkHistoryDto>.Fail("Validation failed", "VALIDATION_ERROR", ModelState));

        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "system";
        var result = await _officerService.UpdateWorkHistoryAsync(id, workHistoryId, request, userId);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    /// <summary>
    /// Delete work history entry
    /// </summary>
    [HttpDelete("{id:long}/work-history/{workHistoryId:long}")]
    public async Task<IActionResult> DeleteWorkHistory(long id, long workHistoryId)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "system";
        var result = await _officerService.DeleteWorkHistoryAsync(id, workHistoryId, userId);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    /// <summary>
    /// Update officer status
    /// </summary>
    [HttpPut("{id:long}/status")]
    public async Task<IActionResult> UpdateStatus(long id, [FromBody] UpdateOfficerStatusRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<OfficerDto>.Fail("Validation failed", "VALIDATION_ERROR", ModelState));

        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "system";
        var result = await _officerService.UpdateStatusAsync(id, request, userId);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    /// <summary>
    /// Get officer status history
    /// </summary>
    [HttpGet("{id:long}/status-history")]
    public async Task<IActionResult> GetStatusHistory(long id)
    {
        var result = await _officerService.GetStatusHistoryAsync(id);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    /// <summary>
    /// Bulk create officers
    /// </summary>
    [HttpPost("bulk")]
    public async Task<IActionResult> BulkCreate([FromBody] BulkCreateOfficersRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<object>.Fail("Validation failed", "VALIDATION_ERROR", ModelState));

        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "system";
        var result = await _officerService.BulkCreateAsync(request, userId);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    /// <summary>
    /// Bulk update officers
    /// </summary>
    [HttpPut("bulk")]
    public async Task<IActionResult> BulkUpdate([FromBody] BulkUpdateOfficersRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<object>.Fail("Validation failed", "VALIDATION_ERROR", ModelState));

        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "system";
        var result = await _officerService.BulkUpdateAsync(request, userId);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    /// <summary>
    /// Bulk delete officers
    /// </summary>
    [HttpDelete("bulk")]
    public async Task<IActionResult> BulkDelete([FromBody] BulkDeleteOfficersRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<object>.Fail("Validation failed", "VALIDATION_ERROR", ModelState));

        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "system";
        var result = await _officerService.BulkDeleteAsync(request, userId);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    /// <summary>
    /// Advanced search officers
    /// </summary>
    [HttpPost("search")]
    public async Task<IActionResult> AdvancedSearch([FromBody] AdvancedSearchRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<object>.Fail("Validation failed", "VALIDATION_ERROR", ModelState));

        var result = await _officerService.AdvancedSearchAsync(request);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    /// <summary>
    /// Get officers statistics
    /// </summary>
    [HttpGet("statistics")]
    public async Task<IActionResult> GetStatistics(
        [FromQuery] string? department = null,
        [FromQuery] string? gradeLevel = null,
        [FromQuery] DateTime? dateFrom = null,
        [FromQuery] DateTime? dateTo = null)
    {
        var result = await _officerService.GetStatisticsAsync(department, gradeLevel, dateFrom, dateTo);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    /// <summary>
    /// Validate officer data
    /// </summary>
    [HttpPost("validate")]
    public async Task<IActionResult> Validate([FromBody] ValidateOfficerDataRequest request)
    {
        var result = await _officerService.ValidateOfficerDataAsync(request.StaffId, request.Email, request.ExcludeId);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    /// <summary>
    /// Export officers
    /// </summary>
    [HttpGet("export")]
    public async Task<IActionResult> Export(
        [FromQuery] string format = "csv",
        [FromQuery] string? search = null,
        [FromQuery] string? department = null,
        [FromQuery] string? gradeLevel = null,
        [FromQuery] string? status = null,
        [FromQuery] bool includeQualifications = false,
        [FromQuery] bool includeWorkHistory = false)
    {
        return await _officerService.ExportAsync(format, search, department, gradeLevel, status, includeQualifications, includeWorkHistory);
    }
}

// Additional request model for validation endpoint
public class ValidateOfficerDataRequest
{
    public string? StaffId { get; set; }
    public string? Email { get; set; }
    public long? ExcludeId { get; set; }
}
