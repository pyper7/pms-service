using PMS.Application.Models.Officers;
using PMS.Domain.Entities;

namespace PMS.Application.Interfaces;

public interface IOfficerRepository
{
    // Basic CRUD
    Task<User?> GetByIdAsync(long id);
    Task<User?> GetByIdWithDetailsAsync(long id);
    Task<IEnumerable<User>> GetAllAsync();
    Task<IEnumerable<User>> GetAllWithDetailsAsync();
    Task<User> CreateAsync(User user);
    Task<User> UpdateAsync(User user);
    Task DeleteAsync(long id);
    
    // Search and Filtering
    Task<(IEnumerable<User> users, int total)> SearchAsync(
        int page, int limit, string? search, string? department, 
        string? gradeLevel, string? status, string? position,
        string? sortBy, string? sortOrder);
    
    Task<(IEnumerable<User> users, int total)> AdvancedSearchAsync(SearchCriteriaDto criteria, int page, int limit, string? sortBy, string? sortOrder);
    
    // Statistics
    Task<OfficerStatisticsDto> GetStatisticsAsync(string? department, string? gradeLevel, DateTime? dateFrom, DateTime? dateTo);
    
    // Qualifications
    Task<IEnumerable<Qualification>> GetQualificationsAsync(long userId);
    Task<Qualification?> GetQualificationByIdAsync(long id);
    Task<Qualification> CreateQualificationAsync(Qualification qualification);
    Task<Qualification> UpdateQualificationAsync(Qualification qualification);
    Task DeleteQualificationAsync(long id);
    
    // Work History
    Task<IEnumerable<WorkHistory>> GetWorkHistoryAsync(long userId);
    Task<WorkHistory?> GetWorkHistoryByIdAsync(long id);
    Task<WorkHistory> CreateWorkHistoryAsync(WorkHistory workHistory);
    Task<WorkHistory> UpdateWorkHistoryAsync(WorkHistory workHistory);
    Task DeleteWorkHistoryAsync(long id);
    
    // Status Management
    Task<IEnumerable<StatusHistory>> GetStatusHistoryAsync(long userId);
    Task<StatusHistory> CreateStatusHistoryAsync(StatusHistory statusHistory);
    
    // Validation
    Task<bool> StaffIdExistsAsync(string staffId, long? excludeId = null);
    Task<bool> EmailExistsAsync(string email, long? excludeId = null);
    
    // Bulk Operations
    Task<IEnumerable<User>> BulkCreateAsync(IEnumerable<User> users);
    Task<IEnumerable<User>> BulkUpdateAsync(IEnumerable<User> users);
    Task BulkDeleteAsync(IEnumerable<long> ids);
    
    // Export
    Task<IEnumerable<User>> GetForExportAsync(string? search, string? department, string? gradeLevel, string? status, bool includeQualifications, bool includeWorkHistory);
}