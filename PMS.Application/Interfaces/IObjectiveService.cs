using PMS.Application.Models;
using PMS.Domain.Entities;

namespace PMS.Application.Interfaces;

public interface IObjectiveService
{
    Task<ApiResponse<Objective>> CreateAsync(long kraId, Objective input, string createdBy);
    Task<ApiResponse<List<Objective>>> ListByKraAsync(long kraId);
    Task<ApiResponse<Objective>> GetByIdAsync(long id);
    Task<ApiResponse<Objective>> UpdateAsync(long id, Objective input, string updatedBy);
    Task<ApiResponse<object>> DeleteAsync(long id, bool force, string deletedBy);

    Task<ApiResponse<object>> ValidateObjectiveWeightsAsync(long kraId);
    Task<ApiResponse<Objective>> UpdateAssignedWeightsAsync(long objectiveId, List<(long unitId, string scope, int weight)> weights, string updatedBy);
    Task<ApiResponse<object>> ValidateAssignedWeightsAsync(long objectiveId);
}


