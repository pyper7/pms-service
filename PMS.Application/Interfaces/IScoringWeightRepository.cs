using PMS.Application.Models.AppraisalSettings;
using PMS.Domain.Entities;

namespace PMS.Application.Interfaces;

public interface IScoringWeightRepository : IRepository<ScoringWeight>
{
    Task<ScoringWeightsResponse> GetScoringWeightsAsync();
    Task<ScoringWeight?> GetBySectionAsync(string section);
    Task<bool> UpdateWeightsAsync(List<ScoringWeightItem> weights, string updatedBy);
    Task<bool> ValidateWeightsAsync(List<ScoringWeightItem> weights);
}
