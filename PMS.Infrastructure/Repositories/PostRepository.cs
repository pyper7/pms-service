using Microsoft.EntityFrameworkCore;
using PMS.Application.Interfaces;
using PMS.Domain.Entities;
using PMS.Infrastructure.Data;

namespace PMS.Infrastructure.Repositories;

public class PostRepository : Repository<Post>, IPostRepository
{
    public PostRepository(PmsDbContext context) : base(context)
    {
    }

    public async Task<Post?> GetWithDetailsAsync(long id)
    {
        return await _dbSet
            .Include(p => p.OrgUnit)
            // Cadre removed
            .Include(p => p.Role)
            .Include(p => p.PostOccupancies)
                .ThenInclude(po => po.User)
            .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);
    }

    public async Task<IEnumerable<Post>> GetAllWithDetailsAsync()
    {
        return await _dbSet
            .Include(p => p.OrgUnit)
            .Include(p => p.Role)
            .Include(p => p.PostOccupancies)
                .ThenInclude(po => po.User)
            .Where(p => !p.IsDeleted)
            .ToListAsync();
    }

    public async Task<List<Post>> GetByOrgUnitAsync(long orgUnitId)
    {
        return await _dbSet
            .Include(p => p.OrgUnit)
            .Include(p => p.Role)
            .Where(p => p.OrgUnitId == orgUnitId && !p.IsDeleted)
            .OrderBy(p => p.Title)
            .ToListAsync();
    }

    public async Task<List<Post>> GetByRoleAsync(long roleId)
    {
        return await _dbSet
            .Include(p => p.OrgUnit)
            .Include(p => p.Role)
            .Where(p => p.RoleId == roleId && !p.IsDeleted)
            .OrderBy(p => p.Title)
            .ToListAsync();
    }

    // Removed GetByCadreAsync

    public async Task<bool> TitleExistsAsync(string title, long? excludeId = null)
    {
        var query = _dbSet.Where(p => p.Title == title && !p.IsDeleted);
        if (excludeId.HasValue)
        {
            query = query.Where(p => p.Id != excludeId.Value);
        }
        return await query.AnyAsync();
    }

    public async Task<List<PostOccupancy>> GetActiveOccupanciesAsync(long postId)
    {
        return await _context.PostOccupancies
            .Include(po => po.User)
            .Where(po => po.PostId == postId && po.EndDate == null && !po.IsDeleted)
            .OrderBy(po => po.StartDate)
            .ToListAsync();
    }

    public async Task<PostOccupancy?> GetActiveOccupancyByUserAsync(long postId, long userId)
    {
        return await _context.PostOccupancies
            .Include(po => po.User)
            .FirstOrDefaultAsync(po => po.PostId == postId && po.UserId == userId && po.EndDate == null && !po.IsDeleted);
    }

    public async Task AssignUserToPostAsync(long postId, long userId, DateTime startDate, bool isPrimary, string? remarks, string assignedBy)
    {
        var occupancy = new PostOccupancy
        {
            PostId = postId,
            UserId = userId,
            StartDate = startDate,
            IsPrimary = isPrimary,
            Remarks = remarks,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = assignedBy
        };

        _context.PostOccupancies.Add(occupancy);
        await _context.SaveChangesAsync();
    }

    public async Task RemoveUserFromPostAsync(long postId, long userId, DateTime endDate, string? remarks, string removedBy)
    {
        var occupancy = await GetActiveOccupancyByUserAsync(postId, userId);
        if (occupancy != null)
        {
            occupancy.EndDate = endDate;
            occupancy.Remarks = remarks;
            occupancy.UpdatedAt = DateTime.UtcNow;
            occupancy.UpdatedBy = removedBy;
            await _context.SaveChangesAsync();
        }
    }

    public async Task<IEnumerable<PostOccupancy>> GetAllOccupanciesWithDetailsAsync()
    {
        return await _context.PostOccupancies
            .Include(po => po.Post)
            .Include(po => po.User)
            .Where(po => !po.IsDeleted)
            .OrderByDescending(po => po.StartDate)
            .ToListAsync();
    }

    public async Task<PostOccupancy?> GetOccupancyWithDetailsAsync(long id)
    {
        return await _context.PostOccupancies
            .Include(po => po.Post)
            .Include(po => po.User)
            .FirstOrDefaultAsync(po => po.Id == id && !po.IsDeleted);
    }

    public async Task<PostOccupancy?> GetCurrentOccupancyAsync(long postId)
    {
        return await _context.PostOccupancies
            .Include(po => po.User)
            .FirstOrDefaultAsync(po => po.PostId == postId && po.IsCurrentlyOccupying && !po.IsDeleted);
    }

    public async Task<IEnumerable<PostOccupancy>> GetOccupancyHistoryAsync(long postId, bool includeInactive = true)
    {
        var query = _context.PostOccupancies
            .Include(po => po.Post)
            .Include(po => po.User)
            .Where(po => po.PostId == postId && !po.IsDeleted);

        if (!includeInactive)
        {
            query = query.Where(po => po.IsCurrentlyOccupying);
        }

        return await query
            .OrderByDescending(po => po.StartDate)
            .ToListAsync();
    }
}
