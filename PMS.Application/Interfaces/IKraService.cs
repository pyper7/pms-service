using Microsoft.AspNetCore.Mvc;
using PMS.Application.Models;
using PMS.Domain.Entities;

namespace PMS.Application.Interfaces;

public interface IKraService
{
    Task<ApiResponse<Kra>> CreateAsync(Kra input, string createdBy);
    Task<ApiResponse<Kra>> UpdateAsync(long id, Kra input, string updatedBy);
    Task<ApiResponse<object>> DeleteAsync(long id, bool force, string deletedBy);
    Task<ApiResponse<Kra>> GetByIdAsync(long id);
    Task<ApiResponse<object>> ListAsync(string? workingYearId, string? appraisalPeriodId, int page, int limit, string? sortBy, string? sortOrder, string? q);
    Task<ApiResponse<object>> ValidateKraWeightsAsync(string workingYearId, string appraisalPeriodId);
    Task<ApiResponse<Kra>> AssignOrgUnitsAsync(long kraId, List<(long id, string type)> units, string updatedBy);
    Task<IActionResult> ExportAsync(string workingYearId, string appraisalPeriodId, string format);
}


