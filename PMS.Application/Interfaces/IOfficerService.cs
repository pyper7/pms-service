using Microsoft.AspNetCore.Mvc;
using PMS.Application.Models;
using PMS.Application.Models.Officers;

namespace PMS.Application.Interfaces;

public interface IOfficerService
{
    // Basic CRUD
    Task<ApiResponse<OfficerListDto>> GetAllAsync(int page = 1, int limit = 10, string? search = null, string? department = null, string? gradeLevel = null, string? status = null, string? position = null, string? sortBy = "name", string? sortOrder = "asc");
    Task<ApiResponse<OfficerDto>> GetByIdAsync(long id);
    Task<ApiResponse<OfficerDto>> CreateAsync(CreateOfficerRequest request, string createdBy);
    Task<ApiResponse<OfficerDto>> UpdateAsync(long id, UpdateOfficerRequest request, string updatedBy);
    Task<ApiResponse<object>> DeleteAsync(long id, string deletedBy);
    
    // Qualifications Management
    Task<ApiResponse<List<QualificationDto>>> GetQualificationsAsync(long officerId);
    Task<ApiResponse<QualificationDto>> AddQualificationAsync(long officerId, CreateQualificationRequest request, string createdBy);
    Task<ApiResponse<QualificationDto>> UpdateQualificationAsync(long officerId, long qualificationId, UpdateQualificationRequest request, string updatedBy);
    Task<ApiResponse<object>> DeleteQualificationAsync(long officerId, long qualificationId, string deletedBy);
    
    // Work History Management
    Task<ApiResponse<List<WorkHistoryDto>>> GetWorkHistoryAsync(long officerId);
    Task<ApiResponse<WorkHistoryDto>> AddWorkHistoryAsync(long officerId, CreateWorkHistoryRequest request, string createdBy);
    Task<ApiResponse<WorkHistoryDto>> UpdateWorkHistoryAsync(long officerId, long workHistoryId, CreateWorkHistoryRequest request, string updatedBy);
    Task<ApiResponse<object>> DeleteWorkHistoryAsync(long officerId, long workHistoryId, string deletedBy);
    
    // Status Management
    Task<ApiResponse<OfficerDto>> UpdateStatusAsync(long id, UpdateOfficerStatusRequest request, string updatedBy);
    Task<ApiResponse<List<StatusHistoryDto>>> GetStatusHistoryAsync(long officerId);
    
    // Bulk Operations
    Task<ApiResponse<object>> BulkCreateAsync(BulkCreateOfficersRequest request, string createdBy);
    Task<ApiResponse<object>> BulkUpdateAsync(BulkUpdateOfficersRequest request, string updatedBy);
    Task<ApiResponse<object>> BulkDeleteAsync(BulkDeleteOfficersRequest request, string deletedBy);
    
    // Search and Statistics
    Task<ApiResponse<object>> AdvancedSearchAsync(AdvancedSearchRequest request);
    Task<ApiResponse<OfficerStatisticsDto>> GetStatisticsAsync(string? department = null, string? gradeLevel = null, DateTime? dateFrom = null, DateTime? dateTo = null);
    
    // Validation
    Task<ApiResponse<ValidationResultDto>> ValidateOfficerDataAsync(string? staffId = null, string? email = null, long? excludeId = null);
    
    // Export
    Task<IActionResult> ExportAsync(string format = "csv", string? search = null, string? department = null, string? gradeLevel = null, string? status = null, bool includeQualifications = false, bool includeWorkHistory = false);
}
