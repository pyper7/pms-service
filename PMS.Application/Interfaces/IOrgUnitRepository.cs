using PMS.Domain.Entities;

namespace PMS.Application.Interfaces;

public interface IOrgUnitRepository : IRepository<OrgUnit>
{
    // Basic queries
    Task<OrgUnit?> GetWithDetailsAsync(long id);
    Task<IEnumerable<OrgUnit>> GetAllWithDetailsAsync();
    Task<IEnumerable<OrgUnit>> GetByParentAsync(long parentId);
    Task<IEnumerable<OrgUnit>> GetByTypeAsync(string type);
    Task<IEnumerable<OrgUnit>> GetByLevelAsync(int level);
    Task<bool> NameExistsAsync(string name, long? parentId = null, long? excludeId = null);

    // Hierarchy queries
    Task<IEnumerable<OrgUnit>> GetTreeAsync(bool includeInactive = false, int? maxDepth = null);
    Task<IEnumerable<OrgUnit>> GetChildrenAsync(long parentId);
    Task<IEnumerable<OrgUnit>> GetDescendantsAsync(long parentId);
    Task<IEnumerable<OrgUnit>> GetAncestorsAsync(long unitId);
    Task<int> GetMaxLevelAsync();
    Task<int> GetChildrenCountAsync(long parentId);
    Task<int> GetDescendantsCountAsync(long parentId);

    // Head of Unit queries
    Task<OrgUnitHead?> GetCurrentHeadAsync(long unitId);
    Task<IEnumerable<OrgUnitHead>> GetHeadHistoryAsync(long unitId);
    Task<bool> IsHeadOfUnitAsync(long userId, long unitId);

    // Statistics queries
    Task<int> GetTotalUnitsCountAsync(string? type = null);
    Task<int> GetActiveUnitsCountAsync(string? type = null);
    Task<int> GetInactiveUnitsCountAsync(string? type = null);
    Task<int> GetStaffCountAsync(long unitId);
    Task<int> GetPostsCountAsync(long unitId);
    Task<int> GetVacantPostsCountAsync(long unitId);
    Task<Dictionary<string, int>> GetUnitsByTypeAsync();
    Task<Dictionary<int, int>> GetUnitsByLevelAsync();

    // Hierarchy operations
    Task UpdateLevelsAsync(long parentId, int newLevel);
    Task ReorderSiblingsAsync(long parentId, int newOrder);
    Task<bool> CanMoveToParentAsync(long unitId, long newParentId);
    Task<bool> HasChildrenAsync(long unitId);
    Task<bool> HasStaffAsync(long unitId);
    Task<bool> HasPostsAsync(long unitId);

    // Bulk operations
    Task<IEnumerable<OrgUnit>> BulkCreateAsync(IEnumerable<OrgUnit> units);
    Task BulkUpdateAsync(IEnumerable<OrgUnit> units);
    Task<int> BulkDeleteAsync(IEnumerable<long> unitIds, bool force = false);
}
