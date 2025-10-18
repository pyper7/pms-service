using Microsoft.EntityFrameworkCore;
using PMS.Application.Interfaces;
using PMS.Domain.Entities;
using PMS.Infrastructure.Data;

namespace PMS.Infrastructure.Repositories;

public class UserRepository : Repository<User>, IUserRepository
{
    public UserRepository(PmsDbContext context) : base(context)
    {
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _dbSet
            .FirstOrDefaultAsync(u => u.Email == email && !u.IsDeleted);
    }

    public async Task<User?> GetByStaffIdAsync(string staffId)
    {
        return await _dbSet
            .FirstOrDefaultAsync(u => u.StaffId == staffId && !u.IsDeleted);
    }

    public async Task<User?> GetWithPostsAsync(long id)
    {
        return await _dbSet
            .Include(u => u.PostOccupancies)
                .ThenInclude(po => po.Post)
                    .ThenInclude(p => p.Role)
            .Include(u => u.PostOccupancies)
                .ThenInclude(po => po.Post)
                    .ThenInclude(p => p.OrgUnit)
            .FirstOrDefaultAsync(u => u.Id == id && !u.IsDeleted);
    }

    public async Task<List<string>> GetPermissionNamesAsync(long userId)
    {
        // Get user's active post occupancies
        var userPosts = await _context.PostOccupancies
            .Where(po => po.UserId == userId && po.EndDate == null && !po.IsDeleted)
            .Select(po => po.PostId)
            .ToListAsync();

        // Get roles from those posts
        var postRoles = await _context.Posts
            .Where(p => userPosts.Contains(p.Id))
            .Select(p => p.RoleId)
            .ToListAsync();

        // Get permissions for those roles
        var permissionNames = await _context.RolePermissions
            .Where(rp => postRoles.Contains(rp.RoleId))
            .Join(_context.Permissions, rp => rp.PermissionId, p => p.Id, (rp, p) => p.Name)
            .Distinct()
            .ToListAsync();

        return permissionNames;
    }

    public async Task<bool> EmailExistsAsync(string email)
    {
        return await _dbSet.AnyAsync(u => u.Email == email && !u.IsDeleted);
    }

    public async Task<bool> StaffIdExistsAsync(string staffId)
    {
        return await _dbSet.AnyAsync(u => u.StaffId == staffId && !u.IsDeleted);
    }
}
