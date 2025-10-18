using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PMS.Application.Interfaces;
using PMS.Application.Models;
using PMS.Application.Models.Permissions;

namespace PMS.API.Controllers;

[ApiController]
[Route("api/v1/permissions")]
[Authorize]
public class PermissionsController : ControllerBase
{
    private readonly IPermissionService _permissionService;

    public PermissionsController(IPermissionService permissionService)
    {
        _permissionService = permissionService;
    }

    /// <summary>
    /// Get all permissions in the system
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<PermissionDto>>>> GetAll()
    {
        var result = await _permissionService.GetAllAsync();
        return Ok(result);
    }

    /// <summary>
    /// Get all permissions grouped by module
    /// </summary>
    [HttpGet("grouped")]
    public async Task<ActionResult<ApiResponse<PermissionListResponse>>> GetAllGroupedByModule()
    {
        var result = await _permissionService.GetAllGroupedByModuleAsync();
        return Ok(result);
    }

    /// <summary>
    /// Get permissions by module
    /// </summary>
    [HttpGet("module/{module}")]
    public async Task<ActionResult<ApiResponse<List<PermissionDto>>>> GetByModule(string module)
    {
        var result = await _permissionService.GetByModuleAsync(module);
        if (!result.Success) return NotFound(result);
        return Ok(result);
    }

    /// <summary>
    /// Get permission by ID
    /// </summary>
    [HttpGet("{id:long}")]
    public async Task<ActionResult<ApiResponse<PermissionDto>>> GetById(long id)
    {
        var result = await _permissionService.GetByIdAsync(id);
        if (!result.Success) return NotFound(result);
        return Ok(result);
    }
}
