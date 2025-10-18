using PMS.Domain.Entities;

namespace PMS.Application.Interfaces;

public interface IRoleRepository : IRepository<Role>
{
    Task<Role?> GetWithPermissionsAsync(long id);
    Task<bool> NameExistsAsync(string name, long? excludeId = null);
    Task<List<string>> GetPermissionNamesAsync(long roleId);
    Task AssignPermissionsAsync(long roleId, IEnumerable<long> permissionIds, string assignedBy);
    Task RemoveAllPermissionsAsync(long roleId);
}


