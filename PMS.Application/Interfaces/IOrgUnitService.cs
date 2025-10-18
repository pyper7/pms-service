using Microsoft.AspNetCore.Mvc;
using PMS.Application.Models;
using PMS.Application.Models.OrganizationalUnits;

namespace PMS.Application.Interfaces;

public interface IOrgUnitService
{
    // Basic CRUD Operations
    Task<ApiResponse<OrgUnitListDto>> GetAllAsync(int page = 1, int pageSize = 50, string? search = null, string? type = null, long? parentId = null, string? status = null, int? level = null, string? sortBy = "name", string? sortOrder = "asc");
    Task<ApiResponse<OrgUnitDto>> GetByIdAsync(long id);
    Task<ApiResponse<OrgUnitDto>> CreateAsync(CreateOrgUnitRequest request, string createdBy);
    Task<ApiResponse<OrgUnitDto>> UpdateAsync(long id, UpdateOrgUnitRequest request, string updatedBy);
    Task<ApiResponse<DeleteOrgUnitResponseDto>> DeleteAsync(long id, bool force = false, string deletedBy = "system");

    // Hierarchy Management
    Task<ApiResponse<OrgUnitTreeResponseDto>> GetTreeAsync(bool includeInactive = false, int? maxDepth = null);
    Task<ApiResponse<List<OrgUnitDto>>> GetChildrenAsync(long id);
    Task<ApiResponse<MoveOrgUnitResponseDto>> MoveAsync(long id, MoveOrgUnitRequest request, string movedBy);

    // Head of Unit Management
    Task<ApiResponse<AssignHeadResponseDto>> AssignHeadAsync(long id, AssignHeadRequest request, string assignedBy);
    Task<ApiResponse<RemoveHeadResponseDto>> RemoveHeadAsync(long id, RemoveHeadRequest request, string removedBy);
    Task<ApiResponse<List<HeadOfUnitHistoryDto>>> GetHeadHistoryAsync(long id);

    // Statistics and Metrics
    Task<ApiResponse<OrgUnitStatisticsDto>> GetStatisticsAsync(string? type = null);
    Task<ApiResponse<OrgUnitMetricsDto>> GetMetricsAsync(long id);

    // Bulk Operations
    Task<ApiResponse<BulkOperationResultDto>> BulkCreateAsync(BulkCreateOrgUnitsRequest request, string createdBy);
    Task<ApiResponse<BulkOperationResultDto>> BulkUpdateAsync(BulkUpdateOrgUnitsRequest request, string updatedBy);

    // Export Operations
    Task<IActionResult> ExportAsync(string format = "csv", string? type = null, bool includeInactive = false);
    Task<IActionResult> ExportTreeAsync(string format = "png", int? maxDepth = null);

    // Validation
    Task<ApiResponse<ValidationResultDto>> ValidateAsync(ValidateOrgUnitRequest request);
    Task<ApiResponse<NameAvailabilityDto>> CheckNameAvailabilityAsync(string name, long? parentId = null, long? excludeId = null);
}
