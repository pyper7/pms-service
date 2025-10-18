using Microsoft.EntityFrameworkCore;
using PMS.Application.Interfaces;
using PMS.Domain.Entities;
using PMS.Infrastructure.Data;

namespace PMS.Infrastructure.Repositories;

public class OrgUnitRepository : Repository<OrgUnit>, IOrgUnitRepository
{
    public OrgUnitRepository(PmsDbContext context) : base(context)
    {
    }

    public async Task<OrgUnit?> GetWithDetailsAsync(long id)
    {
        return await _dbSet
            .Include(o => o.Parent)
            .Include(o => o.Children)
            .Include(o => o.Posts)
            .Include(o => o.HeadHistory)
                .ThenInclude(h => h.User)
            .FirstOrDefaultAsync(o => o.Id == id && !o.IsDeleted);
    }

    public async Task<IEnumerable<OrgUnit>> GetAllWithDetailsAsync()
    {
        return await _dbSet
            .Include(o => o.Parent)
            .Include(o => o.Children)
            .Include(o => o.Posts)
            .Include(o => o.HeadHistory)
                .ThenInclude(h => h.User)
            .Where(o => !o.IsDeleted)
            .ToListAsync();
    }


    public async Task<IEnumerable<OrgUnit>> GetByParentAsync(long parentId)
    {
        return await _dbSet
            .Include(o => o.Parent)
            .Where(o => o.ParentId == parentId && !o.IsDeleted)
            .OrderBy(o => o.Order)
            .ToListAsync();
    }

    public async Task<IEnumerable<OrgUnit>> GetByTypeAsync(string type)
    {
        return await _dbSet
            .Include(o => o.Parent)
            .Where(o => o.Type.ToString() == type && !o.IsDeleted)
            .OrderBy(o => o.Level)
            .ThenBy(o => o.Order)
            .ToListAsync();
    }

    public async Task<IEnumerable<OrgUnit>> GetByLevelAsync(int level)
    {
        return await _dbSet
            .Include(o => o.Parent)
            .Where(o => o.Level == level && !o.IsDeleted)
            .OrderBy(o => o.Order)
            .ToListAsync();
    }

    public async Task<bool> NameExistsAsync(string name, long? parentId = null, long? excludeId = null)
    {
        var query = _dbSet.Where(o => o.Name == name && !o.IsDeleted);
        
        if (parentId.HasValue)
        {
            query = query.Where(o => o.ParentId == parentId.Value);
        }
        
        if (excludeId.HasValue)
        {
            query = query.Where(o => o.Id != excludeId.Value);
        }
        
        return await query.AnyAsync();
    }

    public async Task<IEnumerable<OrgUnit>> GetTreeAsync(bool includeInactive = false, int? maxDepth = null)
    {
        var query = _dbSet
            .Include(o => o.Parent)
            .Include(o => o.Children)
            .Where(o => !o.IsDeleted);


        if (!includeInactive)
        {
            query = query.Where(o => o.IsActive);
        }

        if (maxDepth.HasValue)
        {
            query = query.Where(o => o.Level <= maxDepth.Value);
        }

        return await query
            .OrderBy(o => o.Level)
            .ThenBy(o => o.Order)
            .ToListAsync();
    }

    public async Task<IEnumerable<OrgUnit>> GetChildrenAsync(long parentId)
    {
        return await _dbSet
            .Include(o => o.Parent)
            .Where(o => o.ParentId == parentId && !o.IsDeleted)
            .OrderBy(o => o.Order)
            .ToListAsync();
    }

    public async Task<IEnumerable<OrgUnit>> GetDescendantsAsync(long parentId)
    {
        var descendants = new List<OrgUnit>();
        var children = await GetChildrenAsync(parentId);
        
        foreach (var child in children)
        {
            descendants.Add(child);
            var childDescendants = await GetDescendantsAsync(child.Id);
            descendants.AddRange(childDescendants);
        }
        
        return descendants;
    }

    public async Task<IEnumerable<OrgUnit>> GetAncestorsAsync(long unitId)
    {
        var ancestors = new List<OrgUnit>();
        var unit = await _dbSet
            .Include(o => o.Parent)
            .FirstOrDefaultAsync(o => o.Id == unitId && !o.IsDeleted);
        
        while (unit?.ParentId != null)
        {
            unit = await _dbSet
                .Include(o => o.Parent)
                .FirstOrDefaultAsync(o => o.Id == unit.ParentId && !o.IsDeleted);
            
            if (unit != null)
            {
                ancestors.Add(unit);
            }
        }
        
        return ancestors;
    }

    public async Task<int> GetMaxLevelAsync()
    {
        var query = _dbSet.Where(o => !o.IsDeleted);
        
        
        return await query.MaxAsync(o => (int?)o.Level) ?? 0;
    }

    public async Task<int> GetChildrenCountAsync(long parentId)
    {
        return await _dbSet
            .Where(o => o.ParentId == parentId && !o.IsDeleted)
            .CountAsync();
    }

    public async Task<int> GetDescendantsCountAsync(long parentId)
    {
        var count = 0;
        var children = await GetChildrenAsync(parentId);
        
        foreach (var child in children)
        {
            count++;
            count += await GetDescendantsCountAsync(child.Id);
        }
        
        return count;
    }

    public async Task<OrgUnitHead?> GetCurrentHeadAsync(long unitId)
    {
        return await _context.OrgUnitHeads
            .Include(h => h.User)
            .FirstOrDefaultAsync(h => h.OrgUnitId == unitId && h.IsActive && !h.IsDeleted);
    }

    public async Task<IEnumerable<OrgUnitHead>> GetHeadHistoryAsync(long unitId)
    {
        return await _context.OrgUnitHeads
            .Include(h => h.User)
            .Where(h => h.OrgUnitId == unitId && !h.IsDeleted)
            .OrderByDescending(h => h.EffectiveDate)
            .ToListAsync();
    }

    public async Task<bool> IsHeadOfUnitAsync(long userId, long unitId)
    {
        return await _context.OrgUnitHeads
            .AnyAsync(h => h.UserId == userId && h.OrgUnitId == unitId && h.IsActive && !h.IsDeleted);
    }

    public async Task<int> GetTotalUnitsCountAsync(string? type = null)
    {
        var query = _dbSet.Where(o => !o.IsDeleted);
        
       
        
        if (!string.IsNullOrEmpty(type))
        {
            query = query.Where(o => o.Type.ToString() == type);
        }
        
        return await query.CountAsync();
    }

    public async Task<int> GetActiveUnitsCountAsync(string? type = null)
    {
        var query = _dbSet.Where(o => !o.IsDeleted && o.IsActive);
        
        
        if (!string.IsNullOrEmpty(type))
        {
            query = query.Where(o => o.Type.ToString() == type);
        }
        
        return await query.CountAsync();
    }

    public async Task<int> GetInactiveUnitsCountAsync(string? type = null)
    {
        var query = _dbSet.Where(o => !o.IsDeleted && !o.IsActive);
        
        
        if (!string.IsNullOrEmpty(type))
        {
            query = query.Where(o => o.Type.ToString() == type);
        }
        
        return await query.CountAsync();
    }

    public async Task<int> GetStaffCountAsync(long unitId)
    {
        return await _context.PostOccupancies
            .Where(po => po.Post!.OrgUnitId == unitId && po.IsCurrentlyOccupying && !po.IsDeleted)
            .CountAsync();
    }

    public async Task<int> GetPostsCountAsync(long unitId)
    {
        return await _context.Posts
            .Where(p => p.OrgUnitId == unitId && !p.IsDeleted)
            .CountAsync();
    }

    public async Task<int> GetVacantPostsCountAsync(long unitId)
    {
        var totalPosts = await _context.Posts
            .Where(p => p.OrgUnitId == unitId && !p.IsDeleted)
            .CountAsync();
            
        var occupiedPosts = await _context.PostOccupancies
            .Where(po => po.Post!.OrgUnitId == unitId && po.IsCurrentlyOccupying && !po.IsDeleted)
            .Select(po => po.PostId)
            .Distinct()
            .CountAsync();
            
        return totalPosts - occupiedPosts;
    }

    public async Task<Dictionary<string, int>> GetUnitsByTypeAsync()
    {
        var query = _dbSet.Where(o => !o.IsDeleted);
        
        
        return await query
            .GroupBy(o => o.Type.ToString())
            .ToDictionaryAsync(g => g.Key, g => g.Count());
    }

    public async Task<Dictionary<int, int>> GetUnitsByLevelAsync()
    {
        var query = _dbSet.Where(o => !o.IsDeleted);   

        return await query
            .GroupBy(o => o.Level)
            .ToDictionaryAsync(g => g.Key, g => g.Count());
    }

    public async Task UpdateLevelsAsync(long parentId, int newLevel)
    {
        var children = await GetChildrenAsync(parentId);
        
        foreach (var child in children)
        {
            child.Level = newLevel + 1;
            await UpdateAsync(child);
            
            // Recursively update children
            await UpdateLevelsAsync(child.Id, child.Level);
        }
    }

    public async Task ReorderSiblingsAsync(long parentId, int newOrder)
    {
        var siblings = await GetChildrenAsync(parentId);
        var order = 1;
        
        foreach (var sibling in siblings.OrderBy(s => s.Order))
        {
            if (sibling.Id != parentId)
            {
                sibling.Order = order++;
                await UpdateAsync(sibling);
            }
            else
            {
                sibling.Order = newOrder;
                await UpdateAsync(sibling);
            }
        }
    }

    public async Task<bool> CanMoveToParentAsync(long unitId, long newParentId)
    {
        // Check if new parent is not a descendant of the unit being moved
        var descendants = await GetDescendantsAsync(unitId);
        return !descendants.Any(d => d.Id == newParentId);
    }

    public async Task<bool> HasChildrenAsync(long unitId)
    {
        return await _dbSet
            .AnyAsync(o => o.ParentId == unitId && !o.IsDeleted);
    }

    public async Task<bool> HasStaffAsync(long unitId)
    {
        return await _context.PostOccupancies
            .AnyAsync(po => po.Post!.OrgUnitId == unitId && po.IsCurrentlyOccupying && !po.IsDeleted);
    }

    public async Task<bool> HasPostsAsync(long unitId)
    {
        return await _context.Posts
            .AnyAsync(p => p.OrgUnitId == unitId && !p.IsDeleted);
    }

    public async Task<IEnumerable<OrgUnit>> BulkCreateAsync(IEnumerable<OrgUnit> units)
    {
        _dbSet.AddRange(units);
        await _context.SaveChangesAsync();
        return units;
    }

    public async Task BulkUpdateAsync(IEnumerable<OrgUnit> units)
    {
        _dbSet.UpdateRange(units);
        await _context.SaveChangesAsync();
    }

    public async Task<int> BulkDeleteAsync(IEnumerable<long> unitIds, bool force = false)
    {
        var units = await _dbSet
            .Where(o => unitIds.Contains(o.Id) && !o.IsDeleted)
            .ToListAsync();
        
        if (!force)
        {
            // Check if any unit has children or staff
            foreach (var unit in units)
            {
                if (await HasChildrenAsync(unit.Id) || await HasStaffAsync(unit.Id))
                {
                    throw new InvalidOperationException($"Cannot delete unit {unit.Name} as it has children or staff assigned");
                }
            }
        }
        
        foreach (var unit in units)
        {
            unit.IsDeleted = true;
            unit.UpdatedAt = DateTime.UtcNow;
        }
        
        await _context.SaveChangesAsync();
        return units.Count;
    }
}
