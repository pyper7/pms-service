using PMS.Application.Models;
using PMS.Application.Models.AppraisalSettings;

namespace PMS.Application.Interfaces;

public interface IScoringWeightService
{
    Task<ApiResponse<ScoringWeightsResponse>> GetScoringWeightsAsync();
    Task<ApiResponse<ScoringWeightsResponse>> UpdateScoringWeightsAsync(UpdateScoringWeightsRequest request, string updatedBy);
}
