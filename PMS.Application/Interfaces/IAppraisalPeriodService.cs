using PMS.Application.Models;
using PMS.Application.Models.AppraisalSettings;

namespace PMS.Application.Interfaces;

public interface IAppraisalPeriodService
{
    Task<ApiResponse<PeriodListResponse>> GetPeriodsAsync(PeriodFilters filters, int page, int pageSize);
    Task<ApiResponse<AppraisalPeriodDto>> GetPeriodByIdAsync(long id);
    Task<ApiResponse<AppraisalPeriodDto>> CreatePeriodAsync(CreatePeriodRequest request, string createdBy);
    Task<ApiResponse<AppraisalPeriodDto>> UpdatePeriodAsync(long id, UpdatePeriodRequest request, string updatedBy);
    Task<ApiResponse<object>> DeletePeriodAsync(long id);
    Task<ApiResponse<ToggleStatusResponse>> TogglePeriodStatusAsync(long id, string updatedBy);
}
