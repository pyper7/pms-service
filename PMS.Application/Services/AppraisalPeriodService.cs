using PMS.Application.Interfaces;
using PMS.Application.Models;
using PMS.Application.Models.AppraisalSettings;

namespace PMS.Application.Services;

public class AppraisalPeriodService : IAppraisalPeriodService
{
    private readonly IAppraisalPeriodRepository _periodRepository;

    public AppraisalPeriodService(IAppraisalPeriodRepository periodRepository)
    {
        _periodRepository = periodRepository;
    }

    public async Task<ApiResponse<PeriodListResponse>> GetPeriodsAsync(PeriodFilters filters, int page, int pageSize)
    {
        try
        {
            var result = await _periodRepository.GetPeriodsAsync(filters, page, pageSize);
            return ApiResponse<PeriodListResponse>.Ok(result);
        }
        catch (Exception ex)
        {
            return ApiResponse<PeriodListResponse>.Fail($"Error retrieving periods: {ex.Message}", "INTERNAL_ERROR");
        }
    }

    public async Task<ApiResponse<AppraisalPeriodDto>> GetPeriodByIdAsync(long id)
    {
        try
        {
            var period = await _periodRepository.GetByIdAsync(id);
            if (period == null)
            {
                return ApiResponse<AppraisalPeriodDto>.Fail("Period not found", "NOT_FOUND");
            }

            var dto = new AppraisalPeriodDto
            {
                Id = period.Id,
                Name = period.Name,
                StartDate = period.StartDate,
                EndDate = period.EndDate,
                IsActive = period.IsActive,
                CreatedAt = period.CreatedAt
            };

            return ApiResponse<AppraisalPeriodDto>.Ok(dto);
        }
        catch (Exception ex)
        {
            return ApiResponse<AppraisalPeriodDto>.Fail($"Error retrieving period: {ex.Message}", "INTERNAL_ERROR");
        }
    }

    public async Task<ApiResponse<AppraisalPeriodDto>> CreatePeriodAsync(CreatePeriodRequest request, string createdBy)
    {
        try
        {
            // Validate request
            if (string.IsNullOrWhiteSpace(request.Name))
            {
                return ApiResponse<AppraisalPeriodDto>.Fail("Name is required", "VALIDATION_ERROR");
            }

            if (request.StartDate >= request.EndDate)
            {
                return ApiResponse<AppraisalPeriodDto>.Fail("Start date must be before end date", "VALIDATION_ERROR");
            }

            // Check if period with same name exists
            if (await _periodRepository.ExistsByNameAsync(request.Name))
            {
                return ApiResponse<AppraisalPeriodDto>.Fail("Period with this name already exists", "CONFLICT");
            }

            // Check for overlapping periods
            if (await _periodRepository.HasOverlappingPeriodAsync(request.StartDate, request.EndDate))
            {
                return ApiResponse<AppraisalPeriodDto>.Fail("Period overlaps with existing period", "CONFLICT");
            }

            var period = new Domain.Entities.AppraisalPeriod
            {
                Name = request.Name.Trim(),
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                IsActive = request.IsActive,
                CreatedBy = createdBy
            };

            var createdPeriod = await _periodRepository.AddAsync(period);

            var dto = new AppraisalPeriodDto
            {
                Id = createdPeriod.Id,
                Name = createdPeriod.Name,
                StartDate = createdPeriod.StartDate,
                EndDate = createdPeriod.EndDate,
                IsActive = createdPeriod.IsActive,
                CreatedAt = createdPeriod.CreatedAt
            };

            return ApiResponse<AppraisalPeriodDto>.Ok(dto, "Period created successfully");
        }
        catch (Exception ex)
        {
            return ApiResponse<AppraisalPeriodDto>.Fail($"Error creating period: {ex.Message}", "INTERNAL_ERROR");
        }
    }

    public async Task<ApiResponse<AppraisalPeriodDto>> UpdatePeriodAsync(long id, UpdatePeriodRequest request, string updatedBy)
    {
        try
        {
            var period = await _periodRepository.GetByIdAsync(id);
            if (period == null)
            {
                return ApiResponse<AppraisalPeriodDto>.Fail("Period not found", "NOT_FOUND");
            }

            // Validate request
            if (string.IsNullOrWhiteSpace(request.Name))
            {
                return ApiResponse<AppraisalPeriodDto>.Fail("Name is required", "VALIDATION_ERROR");
            }

            if (request.StartDate >= request.EndDate)
            {
                return ApiResponse<AppraisalPeriodDto>.Fail("Start date must be before end date", "VALIDATION_ERROR");
            }

            // Check if period with same name exists (excluding current one)
            if (await _periodRepository.ExistsByNameAsync(request.Name, id))
            {
                return ApiResponse<AppraisalPeriodDto>.Fail("Period with this name already exists", "CONFLICT");
            }

            // Check for overlapping periods (excluding current one)
            if (await _periodRepository.HasOverlappingPeriodAsync(request.StartDate, request.EndDate, id))
            {
                return ApiResponse<AppraisalPeriodDto>.Fail("Period overlaps with existing period", "CONFLICT");
            }

            period.Name = request.Name.Trim();
            period.StartDate = request.StartDate;
            period.EndDate = request.EndDate;
            period.IsActive = request.IsActive;
            period.UpdatedBy = updatedBy;
            period.UpdatedAt = DateTime.UtcNow;

            await _periodRepository.UpdateAsync(period);

            var dto = new AppraisalPeriodDto
            {
                Id = period.Id,
                Name = period.Name,
                StartDate = period.StartDate,
                EndDate = period.EndDate,
                IsActive = period.IsActive,
                CreatedAt = period.CreatedAt
            };

            return ApiResponse<AppraisalPeriodDto>.Ok(dto, "Period updated successfully");
        }
        catch (Exception ex)
        {
            return ApiResponse<AppraisalPeriodDto>.Fail($"Error updating period: {ex.Message}", "INTERNAL_ERROR");
        }
    }

    public async Task<ApiResponse<object>> DeletePeriodAsync(long id)
    {
        try
        {
            var period = await _periodRepository.GetByIdAsync(id);
            if (period == null)
            {
                return ApiResponse<object>.Fail("Period not found", "NOT_FOUND");
            }

            var success = await _periodRepository.DeleteAsync(id);
            if (!success)
            {
                return ApiResponse<object>.Fail("Failed to delete period", "INTERNAL_ERROR");
            }

            return ApiResponse<object>.Ok(null, "Period deleted successfully");
        }
        catch (Exception ex)
        {
            return ApiResponse<object>.Fail($"Error deleting period: {ex.Message}", "INTERNAL_ERROR");
        }
    }

    public async Task<ApiResponse<ToggleStatusResponse>> TogglePeriodStatusAsync(long id, string updatedBy)
    {
        try
        {
            var period = await _periodRepository.GetByIdAsync(id);
            if (period == null)
            {
                return ApiResponse<ToggleStatusResponse>.Fail("Period not found", "NOT_FOUND");
            }

            period.IsActive = !period.IsActive;
            period.UpdatedBy = updatedBy;
            period.UpdatedAt = DateTime.UtcNow;

            await _periodRepository.UpdateAsync(period);

            var response = new ToggleStatusResponse
            {
                Id = period.Id,
                IsActive = period.IsActive,
                UpdatedAt = period.UpdatedAt ?? DateTime.UtcNow
            };

            return ApiResponse<ToggleStatusResponse>.Ok(response, "Period status updated successfully");
        }
        catch (Exception ex)
        {
            return ApiResponse<ToggleStatusResponse>.Fail($"Error toggling period status: {ex.Message}", "INTERNAL_ERROR");
        }
    }
}
