using PMS.Application.Interfaces;
using PMS.Application.Models;
using PMS.Application.Models.AppraisalSettings;

namespace PMS.Application.Services;

public class ScoringWeightService : IScoringWeightService
{
    private readonly IScoringWeightRepository _scoringWeightRepository;

    public ScoringWeightService(IScoringWeightRepository scoringWeightRepository)
    {
        _scoringWeightRepository = scoringWeightRepository;
    }

    public async Task<ApiResponse<ScoringWeightsResponse>> GetScoringWeightsAsync()
    {
        try
        {
            var result = await _scoringWeightRepository.GetScoringWeightsAsync();
            return ApiResponse<ScoringWeightsResponse>.Ok(result);
        }
        catch (Exception ex)
        {
            return ApiResponse<ScoringWeightsResponse>.Fail($"Error retrieving scoring weights: {ex.Message}", "INTERNAL_ERROR");
        }
    }

    public async Task<ApiResponse<ScoringWeightsResponse>> UpdateScoringWeightsAsync(UpdateScoringWeightsRequest request, string updatedBy)
    {
        try
        {
            // Validate request
            if (request.Weights == null || !request.Weights.Any())
            {
                return ApiResponse<ScoringWeightsResponse>.Fail("Weights are required", "VALIDATION_ERROR");
            }

            // Validate weights total to 100
            if (!await _scoringWeightRepository.ValidateWeightsAsync(request.Weights))
            {
                return ApiResponse<ScoringWeightsResponse>.Fail("Total weight must equal 100", "VALIDATION_ERROR");
            }

            // Validate individual weights
            foreach (var weight in request.Weights)
            {
                if (weight.Weight < 0 || weight.Weight > 100)
                {
                    return ApiResponse<ScoringWeightsResponse>.Fail($"Weight for {weight.Section} must be between 0 and 100", "VALIDATION_ERROR");
                }

                if (string.IsNullOrWhiteSpace(weight.Section))
                {
                    return ApiResponse<ScoringWeightsResponse>.Fail("Section is required for all weights", "VALIDATION_ERROR");
                }
            }

            // Validate required sections
            var requiredSections = new[] { "KPI_TASKS", "COMPETENCIES", "PROCESSES" };
            var providedSections = request.Weights.Select(w => w.Section).ToHashSet();
            
            foreach (var requiredSection in requiredSections)
            {
                if (!providedSections.Contains(requiredSection))
                {
                    return ApiResponse<ScoringWeightsResponse>.Fail($"Required section {requiredSection} is missing", "VALIDATION_ERROR");
                }
            }

            var success = await _scoringWeightRepository.UpdateWeightsAsync(request.Weights, updatedBy);
            if (!success)
            {
                return ApiResponse<ScoringWeightsResponse>.Fail("Failed to update scoring weights", "INTERNAL_ERROR");
            }

            var result = await _scoringWeightRepository.GetScoringWeightsAsync();
            return ApiResponse<ScoringWeightsResponse>.Ok(result, "Scoring weights updated successfully");
        }
        catch (Exception ex)
        {
            return ApiResponse<ScoringWeightsResponse>.Fail($"Error updating scoring weights: {ex.Message}", "INTERNAL_ERROR");
        }
    }
}
