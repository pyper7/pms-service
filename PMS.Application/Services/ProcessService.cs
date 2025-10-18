using PMS.Application.Interfaces;
using PMS.Application.Models;
using PMS.Application.Models.AppraisalSettings;

namespace PMS.Application.Services;

public class ProcessService : IProcessService
{
    private readonly IProcessRepository _processRepository;

    public ProcessService(IProcessRepository processRepository)
    {
        _processRepository = processRepository;
    }

    public async Task<ApiResponse<ProcessListResponse>> GetProcessesAsync(ProcessFilters filters, int page, int pageSize)
    {
        try
        {
            var result = await _processRepository.GetProcessesAsync(filters, page, pageSize);
            return ApiResponse<ProcessListResponse>.Ok(result);
        }
        catch (Exception ex)
        {
            return ApiResponse<ProcessListResponse>.Fail($"Error retrieving processes: {ex.Message}", "INTERNAL_ERROR");
        }
    }

    public async Task<ApiResponse<ProcessDto>> GetProcessByIdAsync(long id)
    {
        try
        {
            var process = await _processRepository.GetByIdAsync(id);
            if (process == null)
            {
                return ApiResponse<ProcessDto>.Fail("Process not found", "NOT_FOUND");
            }

            var dto = new ProcessDto
            {
                Id = process.Id,
                Name = process.Name,
                Description = process.Description,
                Weight = process.Weight,
                IsActive = process.IsActive,
                CreatedAt = process.CreatedAt,
                UpdatedAt = process.UpdatedAt
            };

            return ApiResponse<ProcessDto>.Ok(dto);
        }
        catch (Exception ex)
        {
            return ApiResponse<ProcessDto>.Fail($"Error retrieving process: {ex.Message}", "INTERNAL_ERROR");
        }
    }

    public async Task<ApiResponse<ProcessDto>> CreateProcessAsync(CreateProcessRequest request, string createdBy)
    {
        try
        {
            // Validate request
            if (string.IsNullOrWhiteSpace(request.Name))
            {
                return ApiResponse<ProcessDto>.Fail("Name is required", "VALIDATION_ERROR");
            }

            if (request.Weight < 0 || request.Weight > 100)
            {
                return ApiResponse<ProcessDto>.Fail("Weight must be between 0 and 100", "VALIDATION_ERROR");
            }

            // Check if process with same name exists
            if (await _processRepository.ExistsByNameAsync(request.Name))
            {
                return ApiResponse<ProcessDto>.Fail("Process with this name already exists", "CONFLICT");
            }

            var process = new Domain.Entities.Process
            {
                Name = request.Name.Trim(),
                Description = request.Description?.Trim(),
                Weight = request.Weight,
                IsActive = request.IsActive,
                CreatedBy = createdBy
            };

            var createdProcess = await _processRepository.AddAsync(process);

            var dto = new ProcessDto
            {
                Id = createdProcess.Id,
                Name = createdProcess.Name,
                Description = createdProcess.Description,
                Weight = createdProcess.Weight,
                IsActive = createdProcess.IsActive,
                CreatedAt = createdProcess.CreatedAt,
                UpdatedAt = createdProcess.UpdatedAt
            };

            return ApiResponse<ProcessDto>.Ok(dto, "Process created successfully");
        }
        catch (Exception ex)
        {
            return ApiResponse<ProcessDto>.Fail($"Error creating process: {ex.Message}", "INTERNAL_ERROR");
        }
    }

    public async Task<ApiResponse<ProcessDto>> UpdateProcessAsync(long id, UpdateProcessRequest request, string updatedBy)
    {
        try
        {
            var process = await _processRepository.GetByIdAsync(id);
            if (process == null)
            {
                return ApiResponse<ProcessDto>.Fail("Process not found", "NOT_FOUND");
            }

            // Validate request
            if (string.IsNullOrWhiteSpace(request.Name))
            {
                return ApiResponse<ProcessDto>.Fail("Name is required", "VALIDATION_ERROR");
            }

            if (request.Weight < 0 || request.Weight > 100)
            {
                return ApiResponse<ProcessDto>.Fail("Weight must be between 0 and 100", "VALIDATION_ERROR");
            }

            // Check if process with same name exists (excluding current one)
            if (await _processRepository.ExistsByNameAsync(request.Name, id))
            {
                return ApiResponse<ProcessDto>.Fail("Process with this name already exists", "CONFLICT");
            }

            process.Name = request.Name.Trim();
            process.Description = request.Description?.Trim();
            process.Weight = request.Weight;
            process.IsActive = request.IsActive;
            process.UpdatedBy = updatedBy;
            process.UpdatedAt = DateTime.UtcNow;

            await _processRepository.UpdateAsync(process);

            var dto = new ProcessDto
            {
                Id = process.Id,
                Name = process.Name,
                Description = process.Description,
                Weight = process.Weight,
                IsActive = process.IsActive,
                CreatedAt = process.CreatedAt,
                UpdatedAt = process.UpdatedAt
            };

            return ApiResponse<ProcessDto>.Ok(dto, "Process updated successfully");
        }
        catch (Exception ex)
        {
            return ApiResponse<ProcessDto>.Fail($"Error updating process: {ex.Message}", "INTERNAL_ERROR");
        }
    }

    public async Task<ApiResponse<object>> DeleteProcessAsync(long id)
    {
        try
        {
            var process = await _processRepository.GetByIdAsync(id);
            if (process == null)
            {
                return ApiResponse<object>.Fail("Process not found", "NOT_FOUND");
            }

            var success = await _processRepository.DeleteAsync(id);
            if (!success)
            {
                return ApiResponse<object>.Fail("Failed to delete process", "INTERNAL_ERROR");
            }

            return ApiResponse<object>.Ok(null, "Process deleted successfully");
        }
        catch (Exception ex)
        {
            return ApiResponse<object>.Fail($"Error deleting process: {ex.Message}", "INTERNAL_ERROR");
        }
    }

    public async Task<ApiResponse<ToggleStatusResponse>> ToggleProcessStatusAsync(long id, string updatedBy)
    {
        try
        {
            var process = await _processRepository.GetByIdAsync(id);
            if (process == null)
            {
                return ApiResponse<ToggleStatusResponse>.Fail("Process not found", "NOT_FOUND");
            }

            process.IsActive = !process.IsActive;
            process.UpdatedBy = updatedBy;
            process.UpdatedAt = DateTime.UtcNow;

            await _processRepository.UpdateAsync(process);

            var response = new ToggleStatusResponse
            {
                Id = process.Id,
                IsActive = process.IsActive,
                UpdatedAt = process.UpdatedAt ?? DateTime.UtcNow
            };

            return ApiResponse<ToggleStatusResponse>.Ok(response, "Process status updated successfully");
        }
        catch (Exception ex)
        {
            return ApiResponse<ToggleStatusResponse>.Fail($"Error toggling process status: {ex.Message}", "INTERNAL_ERROR");
        }
    }
}
