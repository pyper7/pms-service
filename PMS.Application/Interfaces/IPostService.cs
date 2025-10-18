using Microsoft.AspNetCore.Mvc;
using PMS.Application.Models;
using PMS.Application.Models.Posts;

namespace PMS.Application.Interfaces;

public interface IPostService
{
    // Post Management
    Task<ApiResponse<PostListDto>> GetAllAsync(int page = 1, int limit = 10, string? search = null, long? orgUnitId = null, string? status = null, bool? isOccupied = null, string? gradeLevel = null, string? sortBy = "name", string? sortOrder = "asc");
    Task<ApiResponse<PostDto>> GetByIdAsync(long id);
    Task<ApiResponse<PostDto>> CreateAsync(CreatePostRequest request, string createdBy);
    Task<ApiResponse<PostDto>> UpdateAsync(long id, UpdatePostRequest request, string updatedBy);
    Task<ApiResponse<object>> DeleteAsync(long id, string deletedBy);
    
    // Role Management
    Task<ApiResponse<object>> AssignRoleAsync(long postId, long roleId, string assignedBy);
    Task<ApiResponse<object>> RemoveRoleAsync(long postId, string removedBy);
    
    // Post Occupancy Management
    Task<ApiResponse<PostOccupancyListDto>> GetAllOccupanciesAsync(int page = 1, int limit = 10, string? search = null, long? postId = null, long? officerId = null, bool? isActive = null, DateTime? startDate = null, DateTime? endDate = null, string? sortBy = "startDate", string? sortOrder = "desc");
    Task<ApiResponse<PostOccupancyDto>> GetOccupancyByIdAsync(long id);
    Task<ApiResponse<PostOccupancyDto>> CreateOccupancyAsync(CreatePostOccupancyRequest request, string createdBy);
    Task<ApiResponse<PostOccupancyDto>> UpdateOccupancyAsync(long id, UpdatePostOccupancyRequest request, string updatedBy);
    Task<ApiResponse<PostOccupancyDto>> EndOccupancyAsync(long id, EndPostOccupancyRequest request, string endedBy);
    Task<ApiResponse<object>> DeleteOccupancyAsync(long id, string deletedBy);
    Task<ApiResponse<PostOccupancyListDto>> GetOccupancyHistoryAsync(long postId, int page = 1, int limit = 10, bool includeInactive = true);
    
    // Validation and Availability
    Task<ApiResponse<AssignmentValidationDto>> ValidateAssignmentAsync(long postId, long officerId);
    Task<ApiResponse<PostAvailabilityDto>> CheckAvailabilityAsync(long postId);
    
    // Statistics
    Task<ApiResponse<PostStatisticsDto>> GetStatisticsAsync();
    Task<ApiResponse<OccupancyStatisticsDto>> GetOccupancyStatisticsAsync();
    
    // Export
    Task<IActionResult> ExportAsync(string format = "csv", string? search = null, long? orgUnitId = null, string? status = null, bool? isOccupied = null, string? gradeLevel = null);
    Task<IActionResult> ExportOccupanciesAsync(string format = "csv", string? search = null, long? postId = null, long? officerId = null, bool? isActive = null, DateTime? startDate = null, DateTime? endDate = null);
    
    // Bulk Operations
    Task<ApiResponse<object>> BulkCreateAsync(List<CreatePostRequest> posts);
    Task<ApiResponse<object>> BulkAssignOfficersAsync(List<CreatePostOccupancyRequest> assignments);
}
