using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PMS.Application.Interfaces;
using PMS.Application.Models;
using PMS.Application.Models.Roles;

namespace PMS.API.Controllers;

[ApiController]
[Route("api/v1/roles")]
[Authorize]
public class RolesController : ControllerBase
{
    private readonly IRoleService _roleService;

    public RolesController(IRoleService roleService)
    {
        _roleService = roleService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<RoleDto>>>> GetAll([FromQuery] string? search)
    {
        var result = await _roleService.GetAllAsync(search);
        return Ok(result);
    }

    [HttpGet("{id:long}")]
    public async Task<ActionResult<ApiResponse<RoleDto>>> GetById(long id)
    {
        var result = await _roleService.GetByIdAsync(id);
        if (!result.Success) return NotFound(result);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<RoleDto>>> Create([FromBody] CreateRoleRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<RoleDto>.Fail("Validation failed", "VALIDATION_ERROR", ModelState));

        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "system";
        var result = await _roleService.CreateAsync(request, userId);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    [HttpPut("{id:long}")]
    public async Task<ActionResult<ApiResponse<RoleDto>>> Update(long id, [FromBody] UpdateRoleRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<RoleDto>.Fail("Validation failed", "VALIDATION_ERROR", ModelState));

        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "system";
        var result = await _roleService.UpdateAsync(id, request, userId);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    [HttpDelete("{id:long}")]
    public async Task<ActionResult<ApiResponse<object>>> Delete(long id)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "system";
        var result = await _roleService.DeleteAsync(id, userId);
        if (!result.Success) return NotFound(result);
        return Ok(result);
    }

    [HttpPost("{id:long}/permissions")]
    public async Task<ActionResult<ApiResponse<object>>> AssignPermissions(long id, [FromBody] AssignPermissionsRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<object>.Fail("Validation failed", "VALIDATION_ERROR", ModelState));

        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "system";
        var result = await _roleService.AssignPermissionsAsync(id, request.PermissionNames, userId);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }
}


