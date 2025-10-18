using Microsoft.AspNetCore.Mvc;
using PMS.Application.Interfaces;
using PMS.Application.Models;
using PMS.Domain.Entities;

namespace PMS.API.Controllers;

[ApiController]
public class ObjectivesController : ControllerBase
{
    private readonly IObjectiveService _objectiveService;

    public ObjectivesController(IObjectiveService objectiveService)
    {
        _objectiveService = objectiveService;
    }

    [HttpPost("api/v1/kras/{kraId}/objectives")]
    public async Task<ActionResult<ApiResponse<Objective>>> Create([FromRoute] long kraId, [FromBody] Objective request)
    {
        if (!ModelState.IsValid) return BadRequest(ApiResponse<Objective>.Fail("Invalid request", "VALIDATION_ERROR", ModelState));
        var createdBy = User?.Identity?.Name ?? "system";
        var result = await _objectiveService.CreateAsync(kraId, request, createdBy);
        if (!result.Success) return BadRequest(result);
        return StatusCode(201, result);
    }

    [HttpGet("api/v1/kras/{kraId}/objectives")]
    public async Task<ActionResult<ApiResponse<List<Objective>>>> List([FromRoute] long kraId)
    {
        var result = await _objectiveService.ListByKraAsync(kraId);
        return Ok(result);
    }

    [HttpGet("api/v1/objectives/{id}")]
    public async Task<ActionResult<ApiResponse<Objective>>> GetById([FromRoute] long id)
    {
        var result = await _objectiveService.GetByIdAsync(id);
        if (!result.Success) return NotFound(result);
        return Ok(result);
    }

    [HttpPut("api/v1/objectives/{id}")]
    public async Task<ActionResult<ApiResponse<Objective>>> Update([FromRoute] long id, [FromBody] Objective request)
    {
        if (!ModelState.IsValid) return BadRequest(ApiResponse<Objective>.Fail("Invalid request", "VALIDATION_ERROR", ModelState));
        var updatedBy = User?.Identity?.Name ?? "system";
        var result = await _objectiveService.UpdateAsync(id, request, updatedBy);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    [HttpDelete("api/v1/objectives/{id}")]
    public async Task<ActionResult<ApiResponse<object>>> Delete([FromRoute] long id, [FromQuery] bool force = false)
    {
        var deletedBy = User?.Identity?.Name ?? "system";
        var result = await _objectiveService.DeleteAsync(id, force, deletedBy);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    [HttpPost("api/v1/objectives/validate-weights")]
    public async Task<ActionResult<ApiResponse<object>>> ValidateWeights([FromBody] dynamic body)
    {
        long kraId = (long)body.kraId;
        var result = await _objectiveService.ValidateObjectiveWeightsAsync(kraId);
        return Ok(result);
    }

    [HttpPost("api/v1/objectives/{id}/assigned-weights")]
    public async Task<ActionResult<ApiResponse<Objective>>> UpdateAssignedWeights([FromRoute] long id, [FromBody] AssignedWeightsRequest request)
    {
        var updatedBy = User?.Identity?.Name ?? "system";
        var units = request.Weights.Select(w => (w.UnitId, w.Scope, w.Weight)).ToList();
        var result = await _objectiveService.UpdateAssignedWeightsAsync(id, units, updatedBy);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    [HttpPost("api/v1/objectives/validate-assigned-weights")]
    public async Task<ActionResult<ApiResponse<object>>> ValidateAssignedWeights([FromBody] dynamic body)
    {
        long objectiveId = (long)body.objectiveId;
        var result = await _objectiveService.ValidateAssignedWeightsAsync(objectiveId);
        return Ok(result);
    }
}

public class AssignedWeightsRequest
{
    public List<AssignedWeightDto> Weights { get; set; } = new();
}

public class AssignedWeightDto
{
    public long UnitId { get; set; }
    public string Scope { get; set; } = string.Empty; // DEPT | DIV | BRANCH
    public int Weight { get; set; }
}


