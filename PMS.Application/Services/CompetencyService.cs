using Microsoft.AspNetCore.Http;
using PMS.Application.Interfaces;
using PMS.Application.Models;
using PMS.Application.Models.AppraisalSettings;

namespace PMS.Application.Services;

public class CompetencyService : ICompetencyService
{
    private readonly ICompetencyRepository _competencyRepository;

    public CompetencyService(ICompetencyRepository competencyRepository)
    {
        _competencyRepository = competencyRepository;
    }

    public async Task<ApiResponse<CompetencyListResponse>> GetCompetenciesAsync(CompetencyFilters filters, int page, int pageSize)
    {
        try
        {
            var result = await _competencyRepository.GetCompetenciesAsync(filters, page, pageSize);
            return ApiResponse<CompetencyListResponse>.Ok(result);
        }
        catch (Exception ex)
        {
            return ApiResponse<CompetencyListResponse>.Fail($"Error retrieving competencies: {ex.Message}", "INTERNAL_ERROR");
        }
    }

    public async Task<ApiResponse<CompetencyDto>> GetCompetencyByIdAsync(long id)
    {
        try
        {
            var competency = await _competencyRepository.GetByIdAsync(id);
            if (competency == null)
            {
                return ApiResponse<CompetencyDto>.Fail("Competency not found", "NOT_FOUND");
            }

            var dto = new CompetencyDto
            {
                Id = competency.Id,
                Name = competency.Name,
                Category = competency.Category,
                Description = competency.Description,
                IsActive = competency.IsActive,
                CreatedAt = competency.CreatedAt,
                UpdatedAt = competency.UpdatedAt
            };

            return ApiResponse<CompetencyDto>.Ok(dto);
        }
        catch (Exception ex)
        {
            return ApiResponse<CompetencyDto>.Fail($"Error retrieving competency: {ex.Message}", "INTERNAL_ERROR");
        }
    }

    public async Task<ApiResponse<CompetencyDto>> CreateCompetencyAsync(CreateCompetencyRequest request, string createdBy)
    {
        try
        {
            // Validate request
            if (string.IsNullOrWhiteSpace(request.Name))
            {
                return ApiResponse<CompetencyDto>.Fail("Name is required", "VALIDATION_ERROR");
            }

            if (string.IsNullOrWhiteSpace(request.Category))
            {
                return ApiResponse<CompetencyDto>.Fail("Category is required", "VALIDATION_ERROR");
            }

            var validCategories = new[] { "GENERIC", "FUNCTIONAL", "ETHICS" };
            if (!validCategories.Contains(request.Category))
            {
                return ApiResponse<CompetencyDto>.Fail("Category must be one of: GENERIC, FUNCTIONAL, ETHICS", "VALIDATION_ERROR");
            }

            // Check if competency with same name exists
            if (await _competencyRepository.ExistsByNameAsync(request.Name))
            {
                return ApiResponse<CompetencyDto>.Fail("Competency with this name already exists", "CONFLICT");
            }

            var competency = new Domain.Entities.Competency
            {
                Name = request.Name.Trim(),
                Category = request.Category,
                Description = request.Description?.Trim(),
                IsActive = request.IsActive,
                CreatedBy = createdBy
            };

            var createdCompetency = await _competencyRepository.AddAsync(competency);

            var dto = new CompetencyDto
            {
                Id = createdCompetency.Id,
                Name = createdCompetency.Name,
                Category = createdCompetency.Category,
                Description = createdCompetency.Description,
                IsActive = createdCompetency.IsActive,
                CreatedAt = createdCompetency.CreatedAt,
                UpdatedAt = createdCompetency.UpdatedAt
            };

            return ApiResponse<CompetencyDto>.Ok(dto, "Competency created successfully");
        }
        catch (Exception ex)
        {
            return ApiResponse<CompetencyDto>.Fail($"Error creating competency: {ex.Message}", "INTERNAL_ERROR");
        }
    }

    public async Task<ApiResponse<CompetencyDto>> UpdateCompetencyAsync(long id, UpdateCompetencyRequest request, string updatedBy)
    {
        try
        {
            var competency = await _competencyRepository.GetByIdAsync(id);
            if (competency == null)
            {
                return ApiResponse<CompetencyDto>.Fail("Competency not found", "NOT_FOUND");
            }

            // Validate request
            if (string.IsNullOrWhiteSpace(request.Name))
            {
                return ApiResponse<CompetencyDto>.Fail("Name is required", "VALIDATION_ERROR");
            }

            if (string.IsNullOrWhiteSpace(request.Category))
            {
                return ApiResponse<CompetencyDto>.Fail("Category is required", "VALIDATION_ERROR");
            }

            var validCategories = new[] { "GENERIC", "FUNCTIONAL", "ETHICS" };
            if (!validCategories.Contains(request.Category))
            {
                return ApiResponse<CompetencyDto>.Fail("Category must be one of: GENERIC, FUNCTIONAL, ETHICS", "VALIDATION_ERROR");
            }

            // Check if competency with same name exists (excluding current one)
            if (await _competencyRepository.ExistsByNameAsync(request.Name, id))
            {
                return ApiResponse<CompetencyDto>.Fail("Competency with this name already exists", "CONFLICT");
            }

            competency.Name = request.Name.Trim();
            competency.Category = request.Category;
            competency.Description = request.Description?.Trim();
            competency.IsActive = request.IsActive;
            competency.UpdatedBy = updatedBy;
            competency.UpdatedAt = DateTime.UtcNow;

            await _competencyRepository.UpdateAsync(competency);

            var dto = new CompetencyDto
            {
                Id = competency.Id,
                Name = competency.Name,
                Category = competency.Category,
                Description = competency.Description,
                IsActive = competency.IsActive,
                CreatedAt = competency.CreatedAt,
                UpdatedAt = competency.UpdatedAt
            };

            return ApiResponse<CompetencyDto>.Ok(dto, "Competency updated successfully");
        }
        catch (Exception ex)
        {
            return ApiResponse<CompetencyDto>.Fail($"Error updating competency: {ex.Message}", "INTERNAL_ERROR");
        }
    }

    public async Task<ApiResponse<object>> DeleteCompetencyAsync(long id)
    {
        try
        {
            var competency = await _competencyRepository.GetByIdAsync(id);
            if (competency == null)
            {
                return ApiResponse<object>.Fail("Competency not found", "NOT_FOUND");
            }

            await _competencyRepository.DeleteAsync(competency);

            return ApiResponse<object>.Ok(null, "Competency deleted successfully");
        }
        catch (Exception ex)
        {
            return ApiResponse<object>.Fail($"Error deleting competency: {ex.Message}", "INTERNAL_ERROR");
        }
    }

    public async Task<ApiResponse<ToggleStatusResponse>> ToggleCompetencyStatusAsync(long id, string updatedBy)
    {
        try
        {
            var competency = await _competencyRepository.GetByIdAsync(id);
            if (competency == null)
            {
                return ApiResponse<ToggleStatusResponse>.Fail("Competency not found", "NOT_FOUND");
            }

            competency.IsActive = !competency.IsActive;
            competency.UpdatedBy = updatedBy;
            competency.UpdatedAt = DateTime.UtcNow;

            await _competencyRepository.UpdateAsync(competency);

            var response = new ToggleStatusResponse
            {
                Id = competency.Id,
                IsActive = competency.IsActive,
                UpdatedAt = competency.UpdatedAt ?? DateTime.UtcNow
            };

            return ApiResponse<ToggleStatusResponse>.Ok(response, "Competency status updated successfully");
        }
        catch (Exception ex)
        {
            return ApiResponse<ToggleStatusResponse>.Fail($"Error toggling competency status: {ex.Message}", "INTERNAL_ERROR");
        }
    }

    public async Task<ApiResponse<BulkOperationResponse>> BulkCreateCompetenciesAsync(BulkCreateCompetencyRequest request, string createdBy)
    {
        try
        {
            if (request.Competencies == null || !request.Competencies.Any())
            {
                return ApiResponse<BulkOperationResponse>.Fail("No competencies provided", "VALIDATION_ERROR");
            }

            var result = await _competencyRepository.BulkCreateAsync(request.Competencies, createdBy);
            return ApiResponse<BulkOperationResponse>.Ok(result, "Bulk creation completed successfully");
        }
        catch (Exception ex)
        {
            return ApiResponse<BulkOperationResponse>.Fail($"Error in bulk creation: {ex.Message}", "INTERNAL_ERROR");
        }
    }

    public async Task<ApiResponse<BulkOperationResponse>> BulkUpdateCompetenciesAsync(BulkUpdateCompetencyRequest request, string updatedBy)
    {
        try
        {
            if (request.Competencies == null || !request.Competencies.Any())
            {
                return ApiResponse<BulkOperationResponse>.Fail("No competencies provided", "VALIDATION_ERROR");
            }

            var result = await _competencyRepository.BulkUpdateAsync(request.Competencies, updatedBy);
            return ApiResponse<BulkOperationResponse>.Ok(result, "Bulk update completed successfully");
        }
        catch (Exception ex)
        {
            return ApiResponse<BulkOperationResponse>.Fail($"Error in bulk update: {ex.Message}", "INTERNAL_ERROR");
        }
    }

    public async Task<ApiResponse<object>> ExportCompetenciesAsync(ExportRequest request)
    {
        try
        {
            // This would implement export functionality
            // For now, return a placeholder response
            return ApiResponse<object>.Fail("Export functionality not yet implemented", "NOT_IMPLEMENTED");
        }
        catch (Exception ex)
        {
            return ApiResponse<object>.Fail($"Error exporting competencies: {ex.Message}", "INTERNAL_ERROR");
        }
    }

    public async Task<ApiResponse<ImportResponse>> ImportCompetenciesAsync(IFormFile file, string uploadedBy)
    {
        try
        {
            // This would implement import functionality
            // For now, return a placeholder response
            return ApiResponse<ImportResponse>.Fail("Import functionality not yet implemented", "NOT_IMPLEMENTED");
        }
        catch (Exception ex)
        {
            return ApiResponse<ImportResponse>.Fail($"Error importing competencies: {ex.Message}", "INTERNAL_ERROR");
        }
    }
}
