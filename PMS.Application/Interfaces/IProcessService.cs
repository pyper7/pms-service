using PMS.Application.Models;
using PMS.Application.Models.AppraisalSettings;

namespace PMS.Application.Interfaces;

public interface IProcessService
{
    Task<ApiResponse<ProcessListResponse>> GetProcessesAsync(ProcessFilters filters, int page, int pageSize);
    Task<ApiResponse<ProcessDto>> GetProcessByIdAsync(long id);
    Task<ApiResponse<ProcessDto>> CreateProcessAsync(CreateProcessRequest request, string createdBy);
    Task<ApiResponse<ProcessDto>> UpdateProcessAsync(long id, UpdateProcessRequest request, string updatedBy);
    Task<ApiResponse<object>> DeleteProcessAsync(long id);
    Task<ApiResponse<ToggleStatusResponse>> ToggleProcessStatusAsync(long id, string updatedBy);
}
