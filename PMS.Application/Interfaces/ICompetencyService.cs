using Microsoft.AspNetCore.Http;
using PMS.Application.Models;
using PMS.Application.Models.AppraisalSettings;

namespace PMS.Application.Interfaces;

public interface ICompetencyService
{
    Task<ApiResponse<CompetencyListResponse>> GetCompetenciesAsync(CompetencyFilters filters, int page, int pageSize);
    Task<ApiResponse<CompetencyDto>> GetCompetencyByIdAsync(long id);
    Task<ApiResponse<CompetencyDto>> CreateCompetencyAsync(CreateCompetencyRequest request, string createdBy);
    Task<ApiResponse<CompetencyDto>> UpdateCompetencyAsync(long id, UpdateCompetencyRequest request, string updatedBy);
    Task<ApiResponse<object>> DeleteCompetencyAsync(long id);
    Task<ApiResponse<ToggleStatusResponse>> ToggleCompetencyStatusAsync(long id, string updatedBy);
    Task<ApiResponse<BulkOperationResponse>> BulkCreateCompetenciesAsync(BulkCreateCompetencyRequest request, string createdBy);
    Task<ApiResponse<BulkOperationResponse>> BulkUpdateCompetenciesAsync(BulkUpdateCompetencyRequest request, string updatedBy);
    Task<ApiResponse<object>> ExportCompetenciesAsync(ExportRequest request);
    Task<ApiResponse<ImportResponse>> ImportCompetenciesAsync(IFormFile file, string uploadedBy);
}
