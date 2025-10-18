using PMS.Domain.Entities;

namespace PMS.Application.Interfaces;

public interface IPostRepository : IRepository<Post>
{
    // Post methods
    Task<Post?> GetWithDetailsAsync(long id);
    Task<IEnumerable<Post>> GetAllWithDetailsAsync();
    Task<List<Post>> GetByOrgUnitAsync(long orgUnitId);
    Task<List<Post>> GetByRoleAsync(long roleId);
    // Removed GetByCadreAsync
    Task<bool> TitleExistsAsync(string title, long? excludeId = null);
    
    // Post Occupancy methods
    Task<List<PostOccupancy>> GetActiveOccupanciesAsync(long postId);
    Task<IEnumerable<PostOccupancy>> GetAllOccupanciesWithDetailsAsync();
    Task<PostOccupancy?> GetOccupancyWithDetailsAsync(long id);
    Task<PostOccupancy?> GetActiveOccupancyByUserAsync(long postId, long userId);
    Task<PostOccupancy?> GetCurrentOccupancyAsync(long postId);
    Task<IEnumerable<PostOccupancy>> GetOccupancyHistoryAsync(long postId, bool includeInactive = true);
    Task AssignUserToPostAsync(long postId, long userId, DateTime startDate, bool isPrimary, string? remarks, string assignedBy);
    Task RemoveUserFromPostAsync(long postId, long userId, DateTime endDate, string? remarks, string removedBy);
}
