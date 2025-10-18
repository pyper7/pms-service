using Microsoft.AspNetCore.Mvc;
using PMS.Application.Interfaces;
using PMS.Application.Models;
using PMS.Domain.Entities;

namespace PMS.API.Controllers;

[ApiController]
public class KpisController : ControllerBase
{
    private readonly IKpiService _kpiService;

    public KpisController(IKpiService kpiService)
    {
        _kpiService = kpiService;
    }

    [HttpPost("api/v1/objectives/{objectiveId}/kpis")]
    public async Task<ActionResult<ApiResponse<Kpi>>> Create([FromRoute] long objectiveId, [FromBody] Kpi request)
    {
        if (!ModelState.IsValid) return BadRequest(ApiResponse<Kpi>.Fail("Invalid request", "VALIDATION_ERROR", ModelState));
        var createdBy = User?.Identity?.Name ?? "system";
        var result = await _kpiService.CreateAsync(objectiveId, request, createdBy);
        if (!result.Success) return BadRequest(result);
        return StatusCode(201, result);
    }

    [HttpGet("api/v1/objectives/{objectiveId}/kpis")]
    public async Task<ActionResult<ApiResponse<List<Kpi>>>> List([FromRoute] long objectiveId)
    {
        var result = await _kpiService.ListByObjectiveAsync(objectiveId);
        return Ok(result);
    }

    [HttpGet("api/v1/kpis/{id}")]
    public async Task<ActionResult<ApiResponse<Kpi>>> GetById([FromRoute] long id)
    {
        var result = await _kpiService.GetByIdAsync(id);
        if (!result.Success) return NotFound(result);
        return Ok(result);
    }

    [HttpPut("api/v1/kpis/{id}")]
    public async Task<ActionResult<ApiResponse<Kpi>>> Update([FromRoute] long id, [FromBody] Kpi request)
    {
        if (!ModelState.IsValid) return BadRequest(ApiResponse<Kpi>.Fail("Invalid request", "VALIDATION_ERROR", ModelState));
        var updatedBy = User?.Identity?.Name ?? "system";
        var result = await _kpiService.UpdateAsync(id, request, updatedBy);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    [HttpDelete("api/v1/kpis/{id}")]
    public async Task<ActionResult<ApiResponse<object>>> Delete([FromRoute] long id)
    {
        var deletedBy = User?.Identity?.Name ?? "system";
        var result = await _kpiService.DeleteAsync(id, deletedBy);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    [HttpPost("api/v1/kpis/validate-weights")]
    public async Task<ActionResult<ApiResponse<object>>> ValidateWeights([FromBody] dynamic body)
    {
        long objectiveId = (long)body.objectiveId;
        var result = await _kpiService.ValidateKpiWeightsAsync(objectiveId);
        return Ok(result);
    }
}


