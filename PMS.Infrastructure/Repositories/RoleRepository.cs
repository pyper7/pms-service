using Microsoft.EntityFrameworkCore;
using PMS.Application.Interfaces;
using PMS.Domain.Entities;
using PMS.Infrastructure.Data;

namespace PMS.Infrastructure.Repositories;

public class RoleRepository : Repository<Role>, IRoleRepository
{
    public RoleRepository(PmsDbContext context) : base(context)
    {
    }

    public async Task<Role?> GetWithPermissionsAsync(long id)
    {
        return await _dbSet
            .Include(r => r.Posts)
            .FirstOrDefaultAsync(r => r.Id == id && !r.IsDeleted);
    }

    public async Task<bool> NameExistsAsync(string name, long? excludeId = null)
    {
        var query = _dbSet.Where(r => r.Name == name && !r.IsDeleted);
        if (excludeId.HasValue)
        {
            query = query.Where(r => r.Id != excludeId.Value);
        }
        return await query.AnyAsync();
    }

    public async Task<List<string>> GetPermissionNamesAsync(long roleId)
    {
        var query = from rp in _context.RolePermissions
                    join p in _context.Permissions on rp.PermissionId equals p.Id
                    where rp.RoleId == roleId
                    select p.Name;
        return await query.Distinct().ToListAsync();
    }

    public async Task AssignPermissionsAsync(long roleId, IEnumerable<long> permissionIds, string assignedBy)
    {
        var existing = _context.RolePermissions.Where(rp => rp.RoleId == roleId);
        _context.RolePermissions.RemoveRange(existing);
        foreach (var pid in permissionIds.Distinct())
        {
            _context.RolePermissions.Add(new RolePermission
            {
                RoleId = roleId,
                PermissionId = pid,
                AssignedAt = DateTime.UtcNow,
                AssignedBy = assignedBy
            });
        }
        await _context.SaveChangesAsync();
    }

    public async Task RemoveAllPermissionsAsync(long roleId)
    {
        var existing = _context.RolePermissions.Where(rp => rp.RoleId == roleId);
        _context.RolePermissions.RemoveRange(existing);
        await _context.SaveChangesAsync();
    }
}


