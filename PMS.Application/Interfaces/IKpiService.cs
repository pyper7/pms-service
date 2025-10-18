using PMS.Application.Models;
using PMS.Domain.Entities;

namespace PMS.Application.Interfaces;

public interface IKpiService
{
    Task<ApiResponse<Kpi>> CreateAsync(long objectiveId, Kpi input, string createdBy);
    Task<ApiResponse<List<Kpi>>> ListByObjectiveAsync(long objectiveId);
    Task<ApiResponse<Kpi>> GetByIdAsync(long id);
    Task<ApiResponse<Kpi>> UpdateAsync(long id, Kpi input, string updatedBy);
    Task<ApiResponse<object>> DeleteAsync(long id, string deletedBy);

    Task<ApiResponse<object>> ValidateKpiWeightsAsync(long objectiveId);
}


