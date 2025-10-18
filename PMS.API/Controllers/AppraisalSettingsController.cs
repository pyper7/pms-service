using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PMS.Application.Interfaces;
using PMS.Application.Models;
using PMS.Application.Models.AppraisalSettings;

namespace PMS.API.Controllers;

[ApiController]
[Route("api/v1/appraisal-settings")]
[Authorize]
public class AppraisalSettingsController : ControllerBase
{
    private readonly ICompetencyService _competencyService;
    private readonly IProcessService _processService;
    private readonly IAppraisalPeriodService _periodService;
    private readonly IScoringWeightService _scoringWeightService;
    private readonly INotificationSettingsService _notificationSettingsService;

    public AppraisalSettingsController(
        ICompetencyService competencyService,
        IProcessService processService,
        IAppraisalPeriodService periodService,
        IScoringWeightService scoringWeightService,
        INotificationSettingsService notificationSettingsService)
    {
        _competencyService = competencyService;
        _processService = processService;
        _periodService = periodService;
        _scoringWeightService = scoringWeightService;
        _notificationSettingsService = notificationSettingsService;
    }

    #region Competencies

    /// <summary>
    /// Get all competencies with optional filtering and pagination
    /// </summary>
    [HttpGet("competencies")]
    public async Task<ActionResult<ApiResponse<CompetencyListResponse>>> GetCompetencies(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 6,
        [FromQuery] string search = "",
        [FromQuery] string category = "ALL",
        [FromQuery] bool? isActive = null)
    {
        var filters = new CompetencyFilters
        {
            Search = search,
            Category = category,
            IsActive = isActive
        };

        var result = await _competencyService.GetCompetenciesAsync(filters, page, pageSize);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Get competency by ID
    /// </summary>
    [HttpGet("competencies/{id}")]
    public async Task<ActionResult<ApiResponse<CompetencyDto>>> GetCompetency(long id)
    {
        var result = await _competencyService.GetCompetencyByIdAsync(id);
        return result.Success ? Ok(result) : NotFound(result);
    }

    /// <summary>
    /// Create a new competency
    /// </summary>
    [HttpPost("competencies")]
    public async Task<ActionResult<ApiResponse<CompetencyDto>>> CreateCompetency([FromBody] CreateCompetencyRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<CompetencyDto>.Fail("Invalid request data", "VALIDATION_ERROR", ModelState));
        }

        var createdBy = GetCurrentUserId();
        var result = await _competencyService.CreateCompetencyAsync(request, createdBy);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Update an existing competency
    /// </summary>
    [HttpPut("competencies/{id}")]
    public async Task<ActionResult<ApiResponse<CompetencyDto>>> UpdateCompetency(long id, [FromBody] UpdateCompetencyRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<CompetencyDto>.Fail("Invalid request data", "VALIDATION_ERROR", ModelState));
        }

        var updatedBy = GetCurrentUserId();
        var result = await _competencyService.UpdateCompetencyAsync(id, request, updatedBy);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Delete a competency
    /// </summary>
    [HttpDelete("competencies/{id}")]
    public async Task<ActionResult<ApiResponse<object>>> DeleteCompetency(long id)
    {
        var result = await _competencyService.DeleteCompetencyAsync(id);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Toggle competency status
    /// </summary>
    [HttpPatch("competencies/{id}/toggle-status")]
    public async Task<ActionResult<ApiResponse<ToggleStatusResponse>>> ToggleCompetencyStatus(long id)
    {
        var updatedBy = GetCurrentUserId();
        var result = await _competencyService.ToggleCompetencyStatusAsync(id, updatedBy);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Bulk create competencies
    /// </summary>
    [HttpPost("competencies/bulk")]
    public async Task<ActionResult<ApiResponse<BulkOperationResponse>>> BulkCreateCompetencies([FromBody] BulkCreateCompetencyRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<BulkOperationResponse>.Fail("Invalid request data", "VALIDATION_ERROR", ModelState));
        }

        var createdBy = GetCurrentUserId();
        var result = await _competencyService.BulkCreateCompetenciesAsync(request, createdBy);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Bulk update competencies
    /// </summary>
    [HttpPut("competencies/bulk")]
    public async Task<ActionResult<ApiResponse<BulkOperationResponse>>> BulkUpdateCompetencies([FromBody] BulkUpdateCompetencyRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<BulkOperationResponse>.Fail("Invalid request data", "VALIDATION_ERROR", ModelState));
        }

        var updatedBy = GetCurrentUserId();
        var result = await _competencyService.BulkUpdateCompetenciesAsync(request, updatedBy);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Export competencies
    /// </summary>
    [HttpGet("competencies/export")]
    public async Task<ActionResult<ApiResponse<object>>> ExportCompetencies([FromQuery] ExportRequest request)
    {
        var result = await _competencyService.ExportCompetenciesAsync(request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Import competencies
    /// </summary>
    [HttpPost("competencies/import")]
    public async Task<ActionResult<ApiResponse<ImportResponse>>> ImportCompetencies(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(ApiResponse<ImportResponse>.Fail("File is required", "VALIDATION_ERROR"));
        }

        var uploadedBy = GetCurrentUserId();
        var result = await _competencyService.ImportCompetenciesAsync(file, uploadedBy);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    #endregion

    #region Processes

    /// <summary>
    /// Get all processes with optional filtering and pagination
    /// </summary>
    [HttpGet("processes")]
    public async Task<ActionResult<ApiResponse<ProcessListResponse>>> GetProcesses(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string search = "",
        [FromQuery] bool? isActive = null)
    {
        var filters = new ProcessFilters
        {
            Search = search,
            IsActive = isActive
        };

        var result = await _processService.GetProcessesAsync(filters, page, pageSize);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Get process by ID
    /// </summary>
    [HttpGet("processes/{id}")]
    public async Task<ActionResult<ApiResponse<ProcessDto>>> GetProcess(long id)
    {
        var result = await _processService.GetProcessByIdAsync(id);
        return result.Success ? Ok(result) : NotFound(result);
    }

    /// <summary>
    /// Create a new process
    /// </summary>
    [HttpPost("processes")]
    public async Task<ActionResult<ApiResponse<ProcessDto>>> CreateProcess([FromBody] CreateProcessRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<ProcessDto>.Fail("Invalid request data", "VALIDATION_ERROR", ModelState));
        }

        var createdBy = GetCurrentUserId();
        var result = await _processService.CreateProcessAsync(request, createdBy);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Update an existing process
    /// </summary>
    [HttpPut("processes/{id}")]
    public async Task<ActionResult<ApiResponse<ProcessDto>>> UpdateProcess(long id, [FromBody] UpdateProcessRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<ProcessDto>.Fail("Invalid request data", "VALIDATION_ERROR", ModelState));
        }

        var updatedBy = GetCurrentUserId();
        var result = await _processService.UpdateProcessAsync(id, request, updatedBy);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Delete a process
    /// </summary>
    [HttpDelete("processes/{id}")]
    public async Task<ActionResult<ApiResponse<object>>> DeleteProcess(long id)
    {
        var result = await _processService.DeleteProcessAsync(id);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Toggle process status
    /// </summary>
    [HttpPatch("processes/{id}/toggle-status")]
    public async Task<ActionResult<ApiResponse<ToggleStatusResponse>>> ToggleProcessStatus(long id)
    {
        var updatedBy = GetCurrentUserId();
        var result = await _processService.ToggleProcessStatusAsync(id, updatedBy);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    #endregion

    #region Appraisal Periods

    /// <summary>
    /// Get all appraisal periods with optional filtering and pagination
    /// </summary>
    [HttpGet("periods")]
    public async Task<ActionResult<ApiResponse<PeriodListResponse>>> GetPeriods(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string search = "",
        [FromQuery] bool? isActive = null,
        [FromQuery] int? year = null)
    {
        var filters = new PeriodFilters
        {
            Search = search,
            IsActive = isActive,
            Year = year
        };

        var result = await _periodService.GetPeriodsAsync(filters, page, pageSize);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Get period by ID
    /// </summary>
    [HttpGet("periods/{id}")]
    public async Task<ActionResult<ApiResponse<AppraisalPeriodDto>>> GetPeriod(long id)
    {
        var result = await _periodService.GetPeriodByIdAsync(id);
        return result.Success ? Ok(result) : NotFound(result);
    }

    /// <summary>
    /// Create a new appraisal period
    /// </summary>
    [HttpPost("periods")]
    public async Task<ActionResult<ApiResponse<AppraisalPeriodDto>>> CreatePeriod([FromBody] CreatePeriodRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<AppraisalPeriodDto>.Fail("Invalid request data", "VALIDATION_ERROR", ModelState));
        }

        var createdBy = GetCurrentUserId();
        var result = await _periodService.CreatePeriodAsync(request, createdBy);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Update an existing appraisal period
    /// </summary>
    [HttpPut("periods/{id}")]
    public async Task<ActionResult<ApiResponse<AppraisalPeriodDto>>> UpdatePeriod(long id, [FromBody] UpdatePeriodRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<AppraisalPeriodDto>.Fail("Invalid request data", "VALIDATION_ERROR", ModelState));
        }

        var updatedBy = GetCurrentUserId();
        var result = await _periodService.UpdatePeriodAsync(id, request, updatedBy);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Delete an appraisal period
    /// </summary>
    [HttpDelete("periods/{id}")]
    public async Task<ActionResult<ApiResponse<object>>> DeletePeriod(long id)
    {
        var result = await _periodService.DeletePeriodAsync(id);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Toggle period status
    /// </summary>
    [HttpPatch("periods/{id}/toggle-status")]
    public async Task<ActionResult<ApiResponse<ToggleStatusResponse>>> TogglePeriodStatus(long id)
    {
        var updatedBy = GetCurrentUserId();
        var result = await _periodService.TogglePeriodStatusAsync(id, updatedBy);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    #endregion

    #region Scoring Weights

    /// <summary>
    /// Get current scoring weights configuration
    /// </summary>
    [HttpGet("scoring-weights")]
    public async Task<ActionResult<ApiResponse<ScoringWeightsResponse>>> GetScoringWeights()
    {
        var result = await _scoringWeightService.GetScoringWeightsAsync();
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Update scoring weights configuration
    /// </summary>
    [HttpPut("scoring-weights")]
    public async Task<ActionResult<ApiResponse<ScoringWeightsResponse>>> UpdateScoringWeights([FromBody] UpdateScoringWeightsRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<ScoringWeightsResponse>.Fail("Invalid request data", "VALIDATION_ERROR", ModelState));
        }

        var updatedBy = GetCurrentUserId();
        var result = await _scoringWeightService.UpdateScoringWeightsAsync(request, updatedBy);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    #endregion

    #region Notification Settings

    /// <summary>
    /// Get current notification settings
    /// </summary>
    [HttpGet("notification-settings")]
    public async Task<IActionResult> GetNotificationSettings()
    {
        var result = await _notificationSettingsService.GetNotificationSettingsAsync();
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Update notification settings
    /// </summary>
    [HttpPut("notification-settings")]
    public async Task<IActionResult> UpdateNotificationSettings([FromBody] UpdateNotificationSettingsRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<NotificationSettingsDto>.Fail("Invalid request data", "VALIDATION_ERROR", ModelState));
        }

        var updatedBy = GetCurrentUserId();
        var result = await _notificationSettingsService.UpdateNotificationSettingsAsync(request, updatedBy);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    #endregion

    private string GetCurrentUserId()
    {
        return User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
            ?? User.FindFirst("nameid")?.Value
            ?? "system";
    }
}
