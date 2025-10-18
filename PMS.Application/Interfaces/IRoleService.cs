using PMS.Application.Models;
using PMS.Application.Models.Roles;

namespace PMS.Application.Interfaces;

public interface IRoleService
{
    Task<ApiResponse<List<RoleDto>>> GetAllAsync(string? search = null);
    Task<ApiResponse<RoleDto>> GetByIdAsync(long id);
    Task<ApiResponse<RoleDto>> CreateAsync(CreateRoleRequest request, string createdBy);
    Task<ApiResponse<RoleDto>> UpdateAsync(long id, UpdateRoleRequest request, string updatedBy);
    Task<ApiResponse<object>> DeleteAsync(long id, string deletedBy);
    Task<ApiResponse<object>> AssignPermissionsAsync(long id, IEnumerable<string> permissionNames, string assignedBy);

}


