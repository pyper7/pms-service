using PMS.Application.Models;
using PMS.Application.Models.Permissions;

namespace PMS.Application.Interfaces;

public interface IPermissionService
{
    Task<ApiResponse<List<PermissionDto>>> GetAllAsync();
    Task<ApiResponse<PermissionListResponse>> GetAllGroupedByModuleAsync();
    Task<ApiResponse<List<PermissionDto>>> GetByModuleAsync(string module);
    Task<ApiResponse<PermissionDto>> GetByIdAsync(long id);
}
