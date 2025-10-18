using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using PMS.Application.Interfaces;
using PMS.Application.Models;
using PMS.Application.Models.Officers;
using PMS.Application.Models.Posts;
using PMS.Domain.Entities;

namespace PMS.Application.Services;

public class OfficerService : IOfficerService
{
    private readonly IOfficerRepository _officerRepository;
    private readonly IEmailQueueService _emailQueueService;
    private readonly IConfiguration _configuration;

    public OfficerService(IOfficerRepository officerRepository, IEmailQueueService emailQueueService, IConfiguration configuration)
    {
        _officerRepository = officerRepository;
        _emailQueueService = emailQueueService;
        _configuration = configuration;
    }

    public async Task<ApiResponse<OfficerListDto>> GetAllAsync(int page = 1, int limit = 10, string? search = null, string? department = null, string? gradeLevel = null, string? status = null, string? position = null, string? sortBy = "name", string? sortOrder = "asc")
    {
        try
        {
            var (officers, total) = await _officerRepository.SearchAsync(page, limit, search, department, gradeLevel, status, position, sortBy, sortOrder);
            
            var totalPages = (int)Math.Ceiling((double)total / limit);
            var pagination = new PaginationDto
            {
                Total = total,
                Page = page,
                Limit = limit,
                TotalPages = totalPages,
                HasNext = page < totalPages,
                HasPrevious = page > 1
            };

            var officerDtos = officers.Select(MapToDto).ToList();
            var result = new OfficerListDto
            {
                Officers = officerDtos,
                Pagination = pagination
            };

            return ApiResponse<OfficerListDto>.Ok(result, "Officers retrieved successfully");
        }
        catch (Exception ex)
        {
            return ApiResponse<OfficerListDto>.Fail($"Error retrieving officers: {ex.Message}", "INTERNAL_ERROR");
        }
    }

    public async Task<ApiResponse<OfficerDto>> GetByIdAsync(long id)
    {
        try
        {
            var officer = await _officerRepository.GetByIdWithDetailsAsync(id);
            if (officer == null)
                return ApiResponse<OfficerDto>.Fail("Officer not found", "NOT_FOUND");

            var dto = MapToDto(officer);
            return ApiResponse<OfficerDto>.Ok(dto, "Officer retrieved successfully");
        }
        catch (Exception ex)
        {
            return ApiResponse<OfficerDto>.Fail($"Error retrieving officer: {ex.Message}", "INTERNAL_ERROR");
        }
    }

    public async Task<ApiResponse<OfficerDto>> CreateAsync(CreateOfficerRequest request, string createdBy)
    {
        try
        {
            // Validate unique constraints
            if (await _officerRepository.StaffIdExistsAsync(request.StaffId))
                return ApiResponse<OfficerDto>.Fail("Staff ID already exists", "VALIDATION_ERROR");

            if (await _officerRepository.EmailExistsAsync(request.Email))
                return ApiResponse<OfficerDto>.Fail("Email address already exists", "VALIDATION_ERROR");

            var user = new User
            {
                StaffId = request.StaffId,
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                PhoneNumber = request.PhoneNumber ?? string.Empty,
                Department = request.Department ?? string.Empty,
                Position = request.Position ?? string.Empty,
                GradeLevel = request.GradeLevel ?? string.Empty,
                Status = request.Status ?? "active",
                DateOfBirth = request.DateOfBirth,
                EmploymentDate = request.DateOfEmployment,
                Address = request.Address,
                EmergencyContactName = request.EmergencyContact?.Name,
                EmergencyContactPhone = request.EmergencyContact?.Phone,
                EmergencyContactRelationship = request.EmergencyContact?.Relationship,
                UserType = "Staff",
                IsActive = false,
                CreatedBy = createdBy,
                CreatedAt = DateTime.UtcNow
            };

            var createdUser = await _officerRepository.CreateAsync(user);

            // Add qualifications
            if (request.Qualifications.Any())
            {
                foreach (var qual in request.Qualifications)
                {
                    var qualification = new Qualification
                    {
                        UserId = createdUser.Id,
                        Institution = qual.Institution,
                        Degree = qual.Degree,
                        Field = qual.Field,
                        Year = qual.Year,
                        Grade = qual.Grade,
                        CertificateNumber = qual.CertificateNumber,
                        CreatedBy = createdBy,
                        CreatedAt = DateTime.UtcNow
                    };
                    await _officerRepository.CreateQualificationAsync(qualification);
                }
            }

            var result = await _officerRepository.GetByIdWithDetailsAsync(createdUser.Id);
            var dto = MapToDto(result!);

            // Queue welcome email for background processing (non-blocking)
            var onboardingUrl = $"{_configuration["AppSettings:BaseUrl"]}/onboarding?token={createdUser.Id}";
            await _emailQueueService.QueueOfficerWelcomeEmailAsync(createdUser, onboardingUrl);

            return ApiResponse<OfficerDto>.Ok(dto, "Officer created successfully");
        }
        catch (Exception ex)
        {
            return ApiResponse<OfficerDto>.Fail($"Error creating officer: {ex.Message}", "INTERNAL_ERROR");
        }
    }

    public async Task<ApiResponse<OfficerDto>> UpdateAsync(long id, UpdateOfficerRequest request, string updatedBy)
    {
        try
        {
            var user = await _officerRepository.GetByIdAsync(id);
            if (user == null)
                return ApiResponse<OfficerDto>.Fail("Officer not found", "NOT_FOUND");

            // Validate unique constraints
            if (await _officerRepository.EmailExistsAsync(request.Email, id))
                return ApiResponse<OfficerDto>.Fail("Email address already exists", "VALIDATION_ERROR");

            user.FirstName = request.FirstName;
            user.LastName = request.LastName;
            user.Email = request.Email;
            user.PhoneNumber = request.PhoneNumber ?? string.Empty;
            user.Department = request.Department ?? string.Empty;
            user.Position = request.Position ?? string.Empty;
            user.GradeLevel = request.GradeLevel ?? string.Empty;
            user.Status = request.Status ?? "active";
            user.DateOfBirth = request.DateOfBirth;
            user.EmploymentDate = request.DateOfEmployment;
            user.Address = request.Address;
            user.EmergencyContactName = request.EmergencyContact?.Name;
            user.EmergencyContactPhone = request.EmergencyContact?.Phone;
            user.EmergencyContactRelationship = request.EmergencyContact?.Relationship;
            user.UpdatedBy = updatedBy;
            user.UpdatedAt = DateTime.UtcNow;

            await _officerRepository.UpdateAsync(user);

            var result = await _officerRepository.GetByIdWithDetailsAsync(id);
            var dto = MapToDto(result!);
            return ApiResponse<OfficerDto>.Ok(dto, "Officer updated successfully");
        }
        catch (Exception ex)
        {
            return ApiResponse<OfficerDto>.Fail($"Error updating officer: {ex.Message}", "INTERNAL_ERROR");
        }
    }

    public async Task<ApiResponse<object>> DeleteAsync(long id, string deletedBy)
    {
        try
        {
            var officer = await _officerRepository.GetByIdAsync(id);
            if (officer == null)
                return ApiResponse<object>.Fail("Officer not found", "NOT_FOUND");

            await _officerRepository.DeleteAsync(id);
            return ApiResponse<object>.Ok(null, "Officer deleted successfully");
        }
        catch (Exception ex)
        {
            return ApiResponse<object>.Fail($"Error deleting officer: {ex.Message}", "INTERNAL_ERROR");
        }
    }

    public async Task<ApiResponse<List<QualificationDto>>> GetQualificationsAsync(long userId)
    {
        try
        {
            var qualifications = await _officerRepository.GetQualificationsAsync(userId);
            var dtos = qualifications.Select(MapQualificationToDto).ToList();
            return ApiResponse<List<QualificationDto>>.Ok(dtos, "Qualifications retrieved successfully");
        }
        catch (Exception ex)
        {
            return ApiResponse<List<QualificationDto>>.Fail($"Error retrieving qualifications: {ex.Message}", "INTERNAL_ERROR");
        }
    }

    public async Task<ApiResponse<QualificationDto>> AddQualificationAsync(long userId, CreateQualificationRequest request, string createdBy)
    {
        try
        {
            var user = await _officerRepository.GetByIdAsync(userId);
            if (user == null)
                return ApiResponse<QualificationDto>.Fail("Officer not found", "NOT_FOUND");

            var qualification = new Qualification
            {
                UserId = userId,
                Institution = request.Institution,
                Degree = request.Degree,
                Field = request.Field,
                Year = request.Year,
                Grade = request.Grade,
                CertificateNumber = request.CertificateNumber,
                CreatedBy = createdBy,
                CreatedAt = DateTime.UtcNow
            };

            var created = await _officerRepository.CreateQualificationAsync(qualification);
            var dto = MapQualificationToDto(created);
            return ApiResponse<QualificationDto>.Ok(dto, "Qualification added successfully");
        }
        catch (Exception ex)
        {
            return ApiResponse<QualificationDto>.Fail($"Error adding qualification: {ex.Message}", "INTERNAL_ERROR");
        }
    }

    public async Task<ApiResponse<QualificationDto>> UpdateQualificationAsync(long userId, long qualificationId, UpdateQualificationRequest request, string updatedBy)
    {
        try
        {
            var qualification = await _officerRepository.GetQualificationByIdAsync(qualificationId);
            if (qualification == null || qualification.UserId != userId)
                return ApiResponse<QualificationDto>.Fail("Qualification not found", "NOT_FOUND");

            qualification.Institution = request.Institution;
            qualification.Degree = request.Degree;
            qualification.Field = request.Field;
            qualification.Year = request.Year;
            qualification.Grade = request.Grade;
            qualification.CertificateNumber = request.CertificateNumber;
            qualification.UpdatedBy = updatedBy;
            qualification.UpdatedAt = DateTime.UtcNow;

            var updated = await _officerRepository.UpdateQualificationAsync(qualification);
            var dto = MapQualificationToDto(updated);
            return ApiResponse<QualificationDto>.Ok(dto, "Qualification updated successfully");
        }
        catch (Exception ex)
        {
            return ApiResponse<QualificationDto>.Fail($"Error updating qualification: {ex.Message}", "INTERNAL_ERROR");
        }
    }

    public async Task<ApiResponse<object>> DeleteQualificationAsync(long userId, long qualificationId, string deletedBy)
    {
        try
        {
            var qualification = await _officerRepository.GetQualificationByIdAsync(qualificationId);
            if (qualification == null || qualification.UserId != userId)
                return ApiResponse<object>.Fail("Qualification not found", "NOT_FOUND");

            await _officerRepository.DeleteQualificationAsync(qualificationId);
            return ApiResponse<object>.Ok(null, "Qualification deleted successfully");
        }
        catch (Exception ex)
        {
            return ApiResponse<object>.Fail($"Error deleting qualification: {ex.Message}", "INTERNAL_ERROR");
        }
    }

    public async Task<ApiResponse<List<WorkHistoryDto>>> GetWorkHistoryAsync(long userId)
    {
        try
        {
            var workHistory = await _officerRepository.GetWorkHistoryAsync(userId);
            var dtos = workHistory.Select(MapWorkHistoryToDto).ToList();
            return ApiResponse<List<WorkHistoryDto>>.Ok(dtos, "Work history retrieved successfully");
        }
        catch (Exception ex)
        {
            return ApiResponse<List<WorkHistoryDto>>.Fail($"Error retrieving work history: {ex.Message}", "INTERNAL_ERROR");
        }
    }

    public async Task<ApiResponse<WorkHistoryDto>> AddWorkHistoryAsync(long userId, CreateWorkHistoryRequest request, string createdBy)
    {
        try
        {
            var user = await _officerRepository.GetByIdAsync(userId);
            if (user == null)
                return ApiResponse<WorkHistoryDto>.Fail("Officer not found", "NOT_FOUND");

            var workHistory = new WorkHistory
            {
                UserId = userId,
                Position = request.Position,
                Department = request.Department,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                Notes = request.Notes,
                CreatedBy = createdBy,
                CreatedAt = DateTime.UtcNow
            };

            var created = await _officerRepository.CreateWorkHistoryAsync(workHistory);
            var dto = MapWorkHistoryToDto(created);
            return ApiResponse<WorkHistoryDto>.Ok(dto, "Work history entry added successfully");
        }
        catch (Exception ex)
        {
            return ApiResponse<WorkHistoryDto>.Fail($"Error adding work history: {ex.Message}", "INTERNAL_ERROR");
        }
    }

    public async Task<ApiResponse<WorkHistoryDto>> UpdateWorkHistoryAsync(long userId, long workHistoryId, CreateWorkHistoryRequest request, string updatedBy)
    {
        try
        {
            var workHistory = await _officerRepository.GetWorkHistoryByIdAsync(workHistoryId);
            if (workHistory == null || workHistory.UserId != userId)
                return ApiResponse<WorkHistoryDto>.Fail("Work history entry not found", "NOT_FOUND");

            workHistory.Position = request.Position;
            workHistory.Department = request.Department;
            workHistory.StartDate = request.StartDate;
            workHistory.EndDate = request.EndDate;
            workHistory.Notes = request.Notes;
            workHistory.UpdatedBy = updatedBy;
            workHistory.UpdatedAt = DateTime.UtcNow;

            var updated = await _officerRepository.UpdateWorkHistoryAsync(workHistory);
            var dto = MapWorkHistoryToDto(updated);
            return ApiResponse<WorkHistoryDto>.Ok(dto, "Work history entry updated successfully");
        }
        catch (Exception ex)
        {
            return ApiResponse<WorkHistoryDto>.Fail($"Error updating work history: {ex.Message}", "INTERNAL_ERROR");
        }
    }

    public async Task<ApiResponse<object>> DeleteWorkHistoryAsync(long userId, long workHistoryId, string deletedBy)
    {
        try
        {
            var workHistory = await _officerRepository.GetWorkHistoryByIdAsync(workHistoryId);
            if (workHistory == null || workHistory.UserId != userId)
                return ApiResponse<object>.Fail("Work history entry not found", "NOT_FOUND");

            await _officerRepository.DeleteWorkHistoryAsync(workHistoryId);
            return ApiResponse<object>.Ok(null, "Work history entry deleted successfully");
        }
        catch (Exception ex)
        {
            return ApiResponse<object>.Fail($"Error deleting work history: {ex.Message}", "INTERNAL_ERROR");
        }
    }

    public async Task<ApiResponse<OfficerDto>> UpdateStatusAsync(long id, UpdateOfficerStatusRequest request, string updatedBy)
    {
        try
        {
            var user = await _officerRepository.GetByIdAsync(id);
            if (user == null)
                return ApiResponse<OfficerDto>.Fail("Officer not found", "NOT_FOUND");

            var previousStatus = user.Status;
            user.Status = request.Status;
            user.StatusReason = request.Reason;
            user.StatusEffectiveDate = request.EffectiveDate;
            user.StatusNotes = request.Notes;
            user.UpdatedBy = updatedBy;
            user.UpdatedAt = DateTime.UtcNow;

            await _officerRepository.UpdateAsync(user);

            // Create status history entry
            var statusHistory = new StatusHistory
            {
                UserId = id,
                PreviousStatus = previousStatus,
                NewStatus = request.Status,
                Reason = request.Reason,
                EffectiveDate = request.EffectiveDate,
                Notes = request.Notes,
                ChangedBy = updatedBy,
                CreatedAt = DateTime.UtcNow
            };
            await _officerRepository.CreateStatusHistoryAsync(statusHistory);

            var result = await _officerRepository.GetByIdWithDetailsAsync(id);
            var dto = MapToDto(result!);
            return ApiResponse<OfficerDto>.Ok(dto, "Officer status updated successfully");
        }
        catch (Exception ex)
        {
            return ApiResponse<OfficerDto>.Fail($"Error updating officer status: {ex.Message}", "INTERNAL_ERROR");
        }
    }

    public async Task<ApiResponse<List<StatusHistoryDto>>> GetStatusHistoryAsync(long userId)
    {
        try
        {
            var statusHistory = await _officerRepository.GetStatusHistoryAsync(userId);
            var dtos = statusHistory.Select(MapStatusHistoryToDto).ToList();
            return ApiResponse<List<StatusHistoryDto>>.Ok(dtos, "Status history retrieved successfully");
        }
        catch (Exception ex)
        {
            return ApiResponse<List<StatusHistoryDto>>.Fail($"Error retrieving status history: {ex.Message}", "INTERNAL_ERROR");
        }
    }

    public async Task<ApiResponse<object>> BulkCreateAsync(BulkCreateOfficersRequest request, string createdBy)
    {
        try
        {
            var users = new List<User>();
            var errors = new List<string>();

            foreach (var officerRequest in request.Officers)
            {
                // Validate unique constraints
                if (await _officerRepository.StaffIdExistsAsync(officerRequest.StaffId))
                {
                    errors.Add($"Staff ID {officerRequest.StaffId} already exists");
                    continue;
                }

                if (await _officerRepository.EmailExistsAsync(officerRequest.Email))
                {
                    errors.Add($"Email {officerRequest.Email} already exists");
                    continue;
                }

                var user = new User
                {
                    StaffId = officerRequest.StaffId,
                    FirstName = officerRequest.FirstName,
                    LastName = officerRequest.LastName,
                    Email = officerRequest.Email,
                    PhoneNumber = officerRequest.PhoneNumber,
                    Department = officerRequest.Department,
                    Position = officerRequest.Position,
                    GradeLevel = officerRequest.GradeLevel,
                    Status = officerRequest.Status,
                    DateOfBirth = officerRequest.DateOfBirth,
                    EmploymentDate = officerRequest.DateOfEmployment,
                    Address = officerRequest.Address,
                    EmergencyContactName = officerRequest.EmergencyContact?.Name,
                    EmergencyContactPhone = officerRequest.EmergencyContact?.Phone,
                    EmergencyContactRelationship = officerRequest.EmergencyContact?.Relationship,
                    UserType = "Staff",
                    IsActive = true,
                    CreatedBy = createdBy,
                    CreatedAt = DateTime.UtcNow
                };

                users.Add(user);
            }

            if (users.Any())
            {
                await _officerRepository.BulkCreateAsync(users);
            }

            var result = new
            {
                Created = users.Count,
                Failed = errors.Count,
                Errors = errors
            };

            return ApiResponse<object>.Ok(result, "Bulk officers created successfully");
        }
        catch (Exception ex)
        {
            return ApiResponse<object>.Fail($"Error in bulk creation: {ex.Message}", "INTERNAL_ERROR");
        }
    }

    public async Task<ApiResponse<object>> BulkUpdateAsync(BulkUpdateOfficersRequest request, string updatedBy)
    {
        try
        {
            var users = new List<User>();
            var errors = new List<string>();

            foreach (var update in request.Updates)
            {
                var user = await _officerRepository.GetByIdAsync(update.Id);
                if (user == null)
                {
                    errors.Add($"Officer with ID {update.Id} not found");
                    continue;
                }

                if (!string.IsNullOrEmpty(update.Department))
                    user.Department = update.Department;
                if (!string.IsNullOrEmpty(update.GradeLevel))
                    user.GradeLevel = update.GradeLevel;
                if (!string.IsNullOrEmpty(update.Position))
                    user.Position = update.Position;
                if (!string.IsNullOrEmpty(update.Status))
                    user.Status = update.Status;

                user.UpdatedBy = updatedBy;
                user.UpdatedAt = DateTime.UtcNow;
                users.Add(user);
            }

            if (users.Any())
            {
                await _officerRepository.BulkUpdateAsync(users);
            }

            var result = new
            {
                Updated = users.Count,
                Failed = errors.Count,
                Errors = errors
            };

            return ApiResponse<object>.Ok(result, "Bulk officers updated successfully");
        }
        catch (Exception ex)
        {
            return ApiResponse<object>.Fail($"Error in bulk update: {ex.Message}", "INTERNAL_ERROR");
        }
    }

    public async Task<ApiResponse<object>> BulkDeleteAsync(BulkDeleteOfficersRequest request, string deletedBy)
    {
        try
        {
            await _officerRepository.BulkDeleteAsync(request.Ids);
            return ApiResponse<object>.Ok(new { Deleted = request.Ids.Count, Failed = 0 }, "Bulk officers deleted successfully");
        }
        catch (Exception ex)
        {
            return ApiResponse<object>.Fail($"Error in bulk delete: {ex.Message}", "INTERNAL_ERROR");
        }
    }

    public async Task<ApiResponse<object>> AdvancedSearchAsync(AdvancedSearchRequest request)
    {
        try
        {
            var page = request.Pagination?.Page ?? 1;
            var limit = request.Pagination?.Limit ?? 20;
            var sortBy = request.Sorting?.Field ?? "name";
            var sortOrder = request.Sorting?.Order ?? "asc";

            var (officers, total) = await _officerRepository.AdvancedSearchAsync(request.Criteria!, page, limit, sortBy, sortOrder);

            var totalPages = (int)Math.Ceiling((double)total / limit);
            var pagination = new PaginationDto
            {
                Total = total,
                Page = page,
                Limit = limit,
                TotalPages = totalPages,
                HasNext = page < totalPages,
                HasPrevious = page > 1
            };

            var officerDtos = officers.Select(MapToDto).ToList();
            var searchMetadata = new SearchMetadataDto
            {
                SearchTime = "0.045s", // This would be calculated in a real implementation
                TotalMatches = total,
                FiltersApplied = GetFiltersAppliedCount(request.Criteria)
            };

            var result = new
            {
                Officers = officerDtos,
                Pagination = pagination,
                SearchMetadata = searchMetadata
            };

            return ApiResponse<object>.Ok(result, "Advanced search completed successfully");
        }
        catch (Exception ex)
        {
            return ApiResponse<object>.Fail($"Error in advanced search: {ex.Message}", "INTERNAL_ERROR");
        }
    }

    public async Task<ApiResponse<OfficerStatisticsDto>> GetStatisticsAsync(string? department = null, string? gradeLevel = null, DateTime? dateFrom = null, DateTime? dateTo = null)
    {
        try
        {
            var statistics = await _officerRepository.GetStatisticsAsync(department, gradeLevel, dateFrom, dateTo);
            return ApiResponse<OfficerStatisticsDto>.Ok(statistics, "Officers statistics retrieved successfully");
        }
        catch (Exception ex)
        {
            return ApiResponse<OfficerStatisticsDto>.Fail($"Error retrieving statistics: {ex.Message}", "INTERNAL_ERROR");
        }
    }

    public async Task<ApiResponse<ValidationResultDto>> ValidateOfficerDataAsync(string? staffId = null, string? email = null, long? excludeId = null)
    {
        try
        {
            var errors = new List<ValidationErrorDto>();
            var warnings = new List<string>();

            if (!string.IsNullOrEmpty(staffId))
            {
                if (await _officerRepository.StaffIdExistsAsync(staffId, excludeId))
                {
                    errors.Add(new ValidationErrorDto
                    {
                        Field = "staffId",
                        Message = "Staff ID already exists",
                        Code = "UNIQUE_CONSTRAINT"
                    });
                }
            }

            if (!string.IsNullOrEmpty(email))
            {
                if (await _officerRepository.EmailExistsAsync(email, excludeId))
                {
                    errors.Add(new ValidationErrorDto
                    {
                        Field = "email",
                        Message = "Email address already exists",
                        Code = "UNIQUE_CONSTRAINT"
                    });
                }
            }

            var result = new ValidationResultDto
            {
                IsValid = !errors.Any(),
                Errors = errors,
                Warnings = warnings
            };

            return ApiResponse<ValidationResultDto>.Ok(result, "Validation completed");
        }
        catch (Exception ex)
        {
            return ApiResponse<ValidationResultDto>.Fail($"Error validating data: {ex.Message}", "INTERNAL_ERROR");
        }
    }

    public async Task<IActionResult> ExportAsync(string format = "csv", string? search = null, string? department = null, string? gradeLevel = null, string? status = null, bool includeQualifications = false, bool includeWorkHistory = false)
    {
        try
        {
            var officers = await _officerRepository.GetForExportAsync(search, department, gradeLevel, status, includeQualifications, includeWorkHistory);

            if (format.ToLower() == "csv")
            {
                var csv = GenerateCsv(officers, includeQualifications, includeWorkHistory);
                var bytes = System.Text.Encoding.UTF8.GetBytes(csv);
                return new FileContentResult(bytes, "text/csv")
                {
                    FileDownloadName = $"officers_export_{DateTime.Now:yyyy-MM-dd}.csv"
                };
            }

            return new BadRequestObjectResult(ApiResponse<object>.Fail("Unsupported export format", "INVALID_FORMAT"));
        }
        catch (Exception ex)
        {
            return new BadRequestObjectResult(ApiResponse<object>.Fail($"Error exporting officers: {ex.Message}", "EXPORT_ERROR"));
        }
    }

    // Helper Methods
    private static OfficerDto MapToDto(User user)
    {
        return new OfficerDto
        {
            Id = user.Id,
            StaffId = user.StaffId ?? string.Empty,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber ?? string.Empty,
            Department = user.Department ?? string.Empty,
            Position = user.Position ?? string.Empty,
            GradeLevel = user.GradeLevel ?? string.Empty,
            Status = user.Status ?? "active",
            DateOfBirth = user.DateOfBirth,
            DateOfEmployment = user.EmploymentDate,
            Address = user.Address,
            EmergencyContact = user.EmergencyContactName != null ? new EmergencyContactDto
            {
                Name = user.EmergencyContactName,
                Phone = user.EmergencyContactPhone ?? string.Empty,
                Relationship = user.EmergencyContactRelationship ?? string.Empty
            } : null,
            Qualifications = user.Qualifications.Select(MapQualificationToDto).ToList(),
            WorkHistory = user.WorkHistory.Select(MapWorkHistoryToDto).ToList(),
            CurrentPost = user.PostOccupancies?.Where(po => po.EndDate == null).Select(po => new CurrentPostDto
            {
                Id = po.PostId,
                Name = po.Post?.Title ?? string.Empty,
                OrgUnit = po.Post?.OrgUnit?.Name ?? string.Empty,
                AssignedDate = po.StartDate
            }).FirstOrDefault(),
            PerformanceHistory = new PerformanceHistoryDto
            {
                TotalAppraisals = 0, // This would be calculated from actual performance data
                AverageRating = 0.0,
                LastAppraisalDate = null
            },
            DateCreated = user.CreatedAt,
            LastUpdated = user.UpdatedAt ?? user.CreatedAt,
            CreatedBy = user.CreatedBy,
            UpdatedBy = user.UpdatedBy ?? user.CreatedBy
        };
    }

    private static QualificationDto MapQualificationToDto(Qualification qualification)
    {
        return new QualificationDto
        {
            Id = qualification.Id,
            UserId = qualification.UserId,
            Institution = qualification.Institution,
            Degree = qualification.Degree,
            Field = qualification.Field,
            Year = qualification.Year,
            Grade = qualification.Grade,
            CertificateNumber = qualification.CertificateNumber,
            DateCreated = qualification.CreatedAt,
            LastUpdated = qualification.UpdatedAt
        };
    }

    private static WorkHistoryDto MapWorkHistoryToDto(WorkHistory workHistory)
    {
        return new WorkHistoryDto
        {
            Id = workHistory.Id,
            UserId = workHistory.UserId,
            Position = workHistory.Position,
            Department = workHistory.Department,
            StartDate = workHistory.StartDate,
            EndDate = workHistory.EndDate,
            Notes = workHistory.Notes,
            DateCreated = workHistory.CreatedAt
        };
    }

    private static StatusHistoryDto MapStatusHistoryToDto(StatusHistory statusHistory)
    {
        return new StatusHistoryDto
        {
            Id = statusHistory.Id,
            UserId = statusHistory.UserId,
            PreviousStatus = statusHistory.PreviousStatus,
            NewStatus = statusHistory.NewStatus,
            Reason = statusHistory.Reason,
            EffectiveDate = statusHistory.EffectiveDate,
            Notes = statusHistory.Notes,
            ChangedBy = statusHistory.ChangedBy,
            DateChanged = statusHistory.CreatedAt
        };
    }

    private static int GetFiltersAppliedCount(SearchCriteriaDto? criteria)
    {
        if (criteria == null) return 0;

        var count = 0;
        if (!string.IsNullOrEmpty(criteria.Name)) count++;
        if (!string.IsNullOrEmpty(criteria.Department)) count++;
        if (criteria.GradeLevel != null) count++;
        if (criteria.DateOfEmployment != null) count++;
        if (criteria.Qualifications != null) count++;

        return count;
    }

    private static string GenerateCsv(IEnumerable<User> users, bool includeQualifications, bool includeWorkHistory)
    {
        var csv = "Id,StaffId,FirstName,LastName,Email,PhoneNumber,Department,Position,GradeLevel,Status,DateOfBirth,DateOfEmployment,Address\n";
        
        foreach (var user in users)
        {
            csv += $"{user.Id},{user.StaffId},{user.FirstName},{user.LastName},{user.Email},{user.PhoneNumber},{user.Department},{user.Position},{user.GradeLevel},{user.Status},{user.DateOfBirth:yyyy-MM-dd},{user.EmploymentDate:yyyy-MM-dd},{user.Address}\n";
        }

        return csv;
    }
}
