using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PMS.Application.Interfaces;
using PMS.Application.Models;
using PMS.Domain.Entities;

namespace PMS.API.Controllers;

[ApiController]
[Route("api/v1/kras")]
public class KrasController : ControllerBase
{
    private readonly IKraService _kraService;

    public KrasController(IKraService kraService)
    {
        _kraService = kraService;
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<Kra>>> Create([FromBody] Kra request)
    {
        if (!ModelState.IsValid) return BadRequest(ApiResponse<Kra>.Fail("Invalid request", "VALIDATION_ERROR", ModelState));
        var createdBy = User?.Identity?.Name ?? "system";
        var result = await _kraService.CreateAsync(request, createdBy);
        if (!result.Success) return BadRequest(result);
        return StatusCode(201, result);
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<object>>> List([FromQuery] string? workingYearId, [FromQuery] string? appraisalPeriodId,
        [FromQuery] int page = 1, [FromQuery] int limit = 20, [FromQuery] string? sortBy = null, [FromQuery] string? sortOrder = null, [FromQuery] string? q = null)
    {
        var result = await _kraService.ListAsync(workingYearId, appraisalPeriodId, page, limit, sortBy, sortOrder, q);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<Kra>>> GetById([FromRoute] long id)
    {
        var result = await _kraService.GetByIdAsync(id);
        if (!result.Success) return NotFound(result);
        return Ok(result);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<Kra>>> Update([FromRoute] long id, [FromBody] Kra request)
    {
        if (!ModelState.IsValid) return BadRequest(ApiResponse<Kra>.Fail("Invalid request", "VALIDATION_ERROR", ModelState));
        var updatedBy = User?.Identity?.Name ?? "system";
        var result = await _kraService.UpdateAsync(id, request, updatedBy);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse<object>>> Delete([FromRoute] long id, [FromQuery] bool force = false)
    {
        var deletedBy = User?.Identity?.Name ?? "system";
        var result = await _kraService.DeleteAsync(id, force, deletedBy);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    [HttpPost("validate-weights")]
    public async Task<ActionResult<ApiResponse<object>>> ValidateWeights([FromBody] dynamic body)
    {
        string workingYearId = body.workingYearId;
        string appraisalPeriodId = body.appraisalPeriodId;
        var result = await _kraService.ValidateKraWeightsAsync(workingYearId, appraisalPeriodId);
        return Ok(result);
    }

    [HttpPost("{id}/assign")]
    public async Task<ActionResult<ApiResponse<Kra>>> Assign([FromRoute] long id, [FromBody] AssignUnitsRequest request)
    {
        var updatedBy = User?.Identity?.Name ?? "system";
        var units = request.Units.Select(u => (u.Id, u.Type)).ToList();
        var result = await _kraService.AssignOrgUnitsAsync(id, units, updatedBy);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    [HttpGet("export")]
    public async Task<IActionResult> Export([FromQuery] string workingYearId, [FromQuery] string appraisalPeriodId, [FromQuery] string format = "excel")
    {
        return await _kraService.ExportAsync(workingYearId, appraisalPeriodId, format);
    }
}

public class AssignUnitsRequest
{
    public List<AssignUnitDto> Units { get; set; } = new();
}

public class AssignUnitDto
{
    public long Id { get; set; }
    public string Type { get; set; } = string.Empty; // DEPT | DIV | BRANCH
}


