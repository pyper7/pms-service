using PMS.Application.Interfaces;
using PMS.Application.Models;
using PMS.Application.Models.Roles;
using PMS.Domain.Entities;

namespace PMS.Application.Services;

public class RoleService : IRoleService
{
    private readonly IRoleRepository _roleRepository;
    private readonly IRepository<Permission> _permissionRepository;

    public RoleService(IRoleRepository roleRepository, IRepository<Permission> permissionRepository)
    {
        _roleRepository = roleRepository;
        _permissionRepository = permissionRepository;
    }

    public async Task<ApiResponse<List<RoleDto>>> GetAllAsync(string? search = null)
    {
        IEnumerable<Role> rolesEnum;
        if (!string.IsNullOrWhiteSpace(search))
        {
            rolesEnum = await _roleRepository.FindAsync(r => !r.IsDeleted && (r.Name.Contains(search) || (r.Description ?? "").Contains(search)));
        }
        else
        {
            rolesEnum = await _roleRepository.FindAsync(r => !r.IsDeleted);
        }
        var roles = rolesEnum.OrderBy(r => r.Name).ToList();
        var dtos = new List<RoleDto>();
        foreach (var role in roles)
        {
            var perms = await _roleRepository.GetPermissionNamesAsync(role.Id);
            dtos.Add(new RoleDto
            {
                Id = role.Id,
                Name = role.Name,
                Description = role.Description,
                IsActive = role.IsActive,
                Permissions = perms
            });
        }
        return ApiResponse<List<RoleDto>>.Ok(dtos, "Roles retrieved");
    }

    public async Task<ApiResponse<RoleDto>> GetByIdAsync(long id)
    {
        var role = await _roleRepository.GetByIdAsync(id);
        if (role == null || role.IsDeleted)
            return ApiResponse<RoleDto>.Fail("Role not found", "ROLE_NOT_FOUND");

        var perms = await _roleRepository.GetPermissionNamesAsync(role.Id);
        var dto = new RoleDto
        {
            Id = role.Id,
            Name = role.Name,
            Description = role.Description,
            IsActive = role.IsActive,
            Permissions = perms
        };
        return ApiResponse<RoleDto>.Ok(dto, "Role retrieved");
    }

    public async Task<ApiResponse<RoleDto>> CreateAsync(CreateRoleRequest request, string createdBy)
    {
        if (await _roleRepository.NameExistsAsync(request.Name))
            return ApiResponse<RoleDto>.Fail("Role name already exists", "DUPLICATE_NAME");

        var role = new Role
        {
            Name = request.Name,
            Description = request.Description,
            IsActive = request.IsActive,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = createdBy
        };

        await _roleRepository.AddAsync(role);
        return await GetByIdAsync(role.Id);
    }

    public async Task<ApiResponse<RoleDto>> UpdateAsync(long id, UpdateRoleRequest request, string updatedBy)
    {
        var role = await _roleRepository.GetByIdAsync(id);
        if (role == null || role.IsDeleted)
            return ApiResponse<RoleDto>.Fail("Role not found", "ROLE_NOT_FOUND");

        if (await _roleRepository.NameExistsAsync(request.Name, id))
            return ApiResponse<RoleDto>.Fail("Role name already exists", "DUPLICATE_NAME");

        role.Name = request.Name;
        role.Description = request.Description;
        role.IsActive = request.IsActive;
        role.UpdatedAt = DateTime.UtcNow;
        role.UpdatedBy = updatedBy;

        await _roleRepository.UpdateAsync(role);
        return await GetByIdAsync(id);
    }

    public async Task<ApiResponse<object>> DeleteAsync(long id, string deletedBy)
    {
        var role = await _roleRepository.GetByIdAsync(id);
        if (role == null || role.IsDeleted)
            return ApiResponse<object>.Fail("Role not found", "ROLE_NOT_FOUND");

        role.IsDeleted = true;
        role.UpdatedAt = DateTime.UtcNow;
        role.UpdatedBy = deletedBy;
        await _roleRepository.UpdateAsync(role);
        return ApiResponse.Ok("Role deleted");
    }

    public async Task<ApiResponse<object>> AssignPermissionsAsync(long id, IEnumerable<string> permissionNames, string assignedBy)
    {
        var role = await _roleRepository.GetByIdAsync(id);
        if (role == null || role.IsDeleted)
            return ApiResponse<object>.Fail("Role not found", "ROLE_NOT_FOUND");

        var nameSet = permissionNames.Select(n => n.Trim()).Where(n => n != string.Empty).ToHashSet();
        var permissionEntities = await _permissionRepository.FindAsync(p => nameSet.Contains(p.Name));
        var permissionIds = permissionEntities.Select(p => p.Id).ToList();

        await _roleRepository.AssignPermissionsAsync(id, permissionIds, assignedBy);
        return ApiResponse.Ok("Permissions assigned");
    }
}


