using PMS.Application.Interfaces;
using PMS.Application.Models;
using PMS.Application.Models.Permissions;
using PMS.Domain.Entities;

namespace PMS.Application.Services;

public class PermissionService : IPermissionService
{
    private readonly IRepository<Permission> _permissionRepository;

    public PermissionService(IRepository<Permission> permissionRepository)
    {
        _permissionRepository = permissionRepository;
    }

    public async Task<ApiResponse<List<PermissionDto>>> GetAllAsync()
    {
        var permissions = await _permissionRepository.FindAsync(p => !p.IsDeleted);
        var dtos = permissions
            .OrderBy(p => p.Module)
            .ThenBy(p => p.Name)
            .Select(MapToDto)
            .ToList();

        return ApiResponse<List<PermissionDto>>.Ok(dtos, "Permissions retrieved");
    }

    public async Task<ApiResponse<PermissionListResponse>> GetAllGroupedByModuleAsync()
    {
        var permissions = await _permissionRepository.FindAsync(p => !p.IsDeleted);
        var permissionDtos = permissions
            .OrderBy(p => p.Module)
            .ThenBy(p => p.Name)
            .Select(MapToDto)
            .ToList();

        var groupedByModule = permissionDtos
            .GroupBy(p => p.Module ?? "Unknown")
            .ToDictionary(g => g.Key, g => g.ToList());

        var response = new PermissionListResponse
        {
            Permissions = permissionDtos,
            PermissionsByModule = groupedByModule,
            TotalCount = permissionDtos.Count
        };

        return ApiResponse<PermissionListResponse>.Ok(response, "Permissions retrieved and grouped by module");
    }

    public async Task<ApiResponse<List<PermissionDto>>> GetByModuleAsync(string module)
    {
        var permissions = await _permissionRepository.FindAsync(p => !p.IsDeleted && p.Module == module);
        var dtos = permissions
            .OrderBy(p => p.Name)
            .Select(MapToDto)
            .ToList();

        return ApiResponse<List<PermissionDto>>.Ok(dtos, $"Permissions for module '{module}' retrieved");
    }

    public async Task<ApiResponse<PermissionDto>> GetByIdAsync(long id)
    {
        var permission = await _permissionRepository.GetByIdAsync(id);
        if (permission == null || permission.IsDeleted)
            return ApiResponse<PermissionDto>.Fail("Permission not found", "PERMISSION_NOT_FOUND");

        var dto = MapToDto(permission);
        return ApiResponse<PermissionDto>.Ok(dto, "Permission retrieved");
    }

    private static PermissionDto MapToDto(Permission permission)
    {
        return new PermissionDto
        {
            Id = permission.Id,
            Name = permission.Name,
            Description = permission.Description,
            Module = permission.Module
        };
    }
}
