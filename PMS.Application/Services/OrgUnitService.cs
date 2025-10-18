using Microsoft.AspNetCore.Mvc;
using PMS.Application.Interfaces;
using PMS.Application.Models;
using PMS.Application.Models.OrganizationalUnits;
using PMS.Application.Models.Posts;
using PMS.Domain.Entities;

namespace PMS.Application.Services;

public class OrgUnitService : IOrgUnitService
{
    private readonly IOrgUnitRepository _orgUnitRepository;
    private readonly IRepository<User> _userRepository;
    private readonly IRepository<OrgUnitHead> _orgUnitHeadRepository;

    public OrgUnitService(
        IOrgUnitRepository orgUnitRepository,
        IRepository<User> userRepository,
        IRepository<OrgUnitHead> orgUnitHeadRepository)
    {
        _orgUnitRepository = orgUnitRepository;
        _userRepository = userRepository;
        _orgUnitHeadRepository = orgUnitHeadRepository;
    }

    // Basic CRUD Operations
    public async Task<ApiResponse<OrgUnitListDto>> GetAllAsync(int page = 1, int pageSize = 50, string? search = null, string? type = null, long? parentId = null, string? status = null, int? level = null, string? sortBy = "name", string? sortOrder = "asc")
    {
        try
        {
            var units = await _orgUnitRepository.GetAllWithDetailsAsync();
            
            // Apply filters
            if (!string.IsNullOrWhiteSpace(search))
            {
                units = units.Where(u => u.Name.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                                       u.Description?.Contains(search, StringComparison.OrdinalIgnoreCase) == true);
            }
            
            if (!string.IsNullOrWhiteSpace(type))
            {
                units = units.Where(u => u.Type.ToString().Equals(type, StringComparison.OrdinalIgnoreCase));
            }
            
            if (parentId.HasValue)
            {
                units = units.Where(u => u.ParentId == parentId.Value);
            }
            
            if (!string.IsNullOrWhiteSpace(status))
            {
                var isActive = status.ToLower() == "active";
                units = units.Where(u => u.IsActive == isActive);
            }
            
            if (level.HasValue)
            {
                units = units.Where(u => u.Level == level.Value);
            }

            // Apply sorting
            units = sortBy.ToLower() switch
            {
                "name" => sortOrder.ToLower() == "desc" ? units.OrderByDescending(u => u.Name) : units.OrderBy(u => u.Name),
                "type" => sortOrder.ToLower() == "desc" ? units.OrderByDescending(u => u.Type) : units.OrderBy(u => u.Type),
                "level" => sortOrder.ToLower() == "desc" ? units.OrderByDescending(u => u.Level) : units.OrderBy(u => u.Level),
                "order" => sortOrder.ToLower() == "desc" ? units.OrderByDescending(u => u.Order) : units.OrderBy(u => u.Order),
                _ => units.OrderBy(u => u.Name)
            };

            var total = units.Count();
            var totalPages = (int)Math.Ceiling((double)total / pageSize);
            var skip = (page - 1) * pageSize;
            
            var pagedUnits = units.Skip(skip).Take(pageSize).ToList();
            var unitDtos = pagedUnits.Select(MapToDto).ToList();

            var pagination = new PaginationDto
            {
                Total = total,
                Page = page,
                Limit = pageSize,
                TotalPages = totalPages,
                HasNext = page < totalPages,
                HasPrevious = page > 1
            };

            var summary = new OrgUnitSummaryDto
            {
                TotalUnits = total,
                ActiveUnits = units.Count(u => u.IsActive),
                InactiveUnits = units.Count(u => !u.IsActive),
                ByType = units.GroupBy(u => u.Type.ToString())
                    .ToDictionary(g => g.Key, g => g.Count())
            };

            var result = new OrgUnitListDto
            {
                Units = unitDtos,
                Pagination = pagination,
                Summary = summary
            };

            return ApiResponse<OrgUnitListDto>.Ok(result, "Organizational units retrieved successfully");
        }
        catch (Exception ex)
        {
            return ApiResponse<OrgUnitListDto>.Fail($"Error retrieving organizational units: {ex.Message}", "INTERNAL_ERROR");
        }
    }

    public async Task<ApiResponse<OrgUnitDto>> GetByIdAsync(long id)
    {
        try
        {
            var unit = await _orgUnitRepository.GetWithDetailsAsync(id);
            if (unit == null)
                return ApiResponse<OrgUnitDto>.Fail("Organizational unit not found", "NOT_FOUND");

            var dto = MapToDto(unit);
            return ApiResponse<OrgUnitDto>.Ok(dto, "Organizational unit retrieved successfully");
        }
        catch (Exception ex)
        {
            return ApiResponse<OrgUnitDto>.Fail($"Error retrieving organizational unit: {ex.Message}", "INTERNAL_ERROR");
        }
    }

    public async Task<ApiResponse<OrgUnitDto>> CreateAsync(CreateOrgUnitRequest request, string createdBy)
    {
        try
        {
            if (await _orgUnitRepository.NameExistsAsync(request.Name, request.ParentId))
                return ApiResponse<OrgUnitDto>.Fail("Organizational unit name already exists", "DUPLICATE_NAME");

            // Validate parent exists if specified
            OrgUnit? parent = null;
            if (request.ParentId.HasValue)
            {
                parent = await _orgUnitRepository.GetByIdAsync(request.ParentId.Value);
                if (parent == null || parent.IsDeleted)
                    return ApiResponse<OrgUnitDto>.Fail("Parent organizational unit not found", "PARENT_NOT_FOUND");
            }

            var unit = new OrgUnit
            {
                Name = request.Name,
                Type = Enum.Parse<OrgUnitType>(request.Type, true),
                Description = request.Description,
                ParentId = request.ParentId,
                Order = request.Order,
                Level = parent?.Level + 1 ?? 0,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = createdBy
            };

            await _orgUnitRepository.AddAsync(unit);
            return await GetByIdAsync(unit.Id);
        }
        catch (Exception ex)
        {
            return ApiResponse<OrgUnitDto>.Fail($"Error creating organizational unit: {ex.Message}", "INTERNAL_ERROR");
        }
    }

    public async Task<ApiResponse<OrgUnitDto>> UpdateAsync(long id, UpdateOrgUnitRequest request, string updatedBy)
    {
        try
        {
            var unit = await _orgUnitRepository.GetByIdAsync(id);
            if (unit == null || unit.IsDeleted)
                return ApiResponse<OrgUnitDto>.Fail("Organizational unit not found", "NOT_FOUND");

            if (!string.IsNullOrWhiteSpace(request.Name) && await _orgUnitRepository.NameExistsAsync(request.Name, unit.ParentId, id))
                return ApiResponse<OrgUnitDto>.Fail("Organizational unit name already exists", "DUPLICATE_NAME");

            if (!string.IsNullOrWhiteSpace(request.Name))
                unit.Name = request.Name;

            if (request.Description != null)
                unit.Description = request.Description;

            if (request.Order.HasValue)
                unit.Order = request.Order.Value;

            unit.UpdatedAt = DateTime.UtcNow;
            unit.UpdatedBy = updatedBy;

            await _orgUnitRepository.UpdateAsync(unit);
            return await GetByIdAsync(id);
        }
        catch (Exception ex)
        {
            return ApiResponse<OrgUnitDto>.Fail($"Error updating organizational unit: {ex.Message}", "INTERNAL_ERROR");
        }
    }

    public async Task<ApiResponse<DeleteOrgUnitResponseDto>> DeleteAsync(long id, bool force = false, string deletedBy = "system")
    {
        try
        {
            var unit = await _orgUnitRepository.GetWithDetailsAsync(id);
            if (unit == null || unit.IsDeleted)
                return ApiResponse<DeleteOrgUnitResponseDto>.Fail("Organizational unit not found", "NOT_FOUND");

            if (!force)
            {
                if (await _orgUnitRepository.HasChildrenAsync(id))
                    return ApiResponse<DeleteOrgUnitResponseDto>.Fail("Cannot delete unit with children. Use force=true to delete with children.", "HAS_CHILDREN");

                if (await _orgUnitRepository.HasStaffAsync(id))
                    return ApiResponse<DeleteOrgUnitResponseDto>.Fail("Cannot delete unit with assigned staff. Use force=true to delete with staff.", "HAS_STAFF");
            }

            var deletedChildrenCount = await _orgUnitRepository.GetDescendantsCountAsync(id);
            var affectedPostsCount = await _orgUnitRepository.GetPostsCountAsync(id);
            var affectedStaffCount = await _orgUnitRepository.GetStaffCountAsync(id);

            unit.IsDeleted = true;
            unit.UpdatedAt = DateTime.UtcNow;
            unit.UpdatedBy = deletedBy;
            await _orgUnitRepository.UpdateAsync(unit);

            var response = new DeleteOrgUnitResponseDto
            {
                DeletedUnitId = id,
                DeletedChildrenCount = deletedChildrenCount,
                AffectedPostsCount = affectedPostsCount,
                AffectedStaffCount = affectedStaffCount
            };

            return ApiResponse<DeleteOrgUnitResponseDto>.Ok(response, "Organizational unit deleted successfully");
        }
        catch (Exception ex)
        {
            return ApiResponse<DeleteOrgUnitResponseDto>.Fail($"Error deleting organizational unit: {ex.Message}", "INTERNAL_ERROR");
        }
    }

    // Hierarchy Management
    public async Task<ApiResponse<OrgUnitTreeResponseDto>> GetTreeAsync(bool includeInactive = false, int? maxDepth = null)
    {
        try
        {
            var units = await _orgUnitRepository.GetTreeAsync(includeInactive, maxDepth);
            var rootUnits = units.Where(u => u.ParentId == null).ToList();
            
            var tree = rootUnits.Select(BuildTree).ToList();
            
            var summary = new OrgUnitTreeSummaryDto
            {
                TotalUnits = units.Count(),
                MaxDepth = units.Max(u => u.Level),
                ByLevel = units.GroupBy(u => u.Level)
                    .ToDictionary(g => g.Key.ToString(), g => g.Count())
            };

            var result = new OrgUnitTreeResponseDto
            {
                Tree = tree,
                Summary = summary
            };

            return ApiResponse<OrgUnitTreeResponseDto>.Ok(result, "Organizational tree retrieved successfully");
        }
        catch (Exception ex)
        {
            return ApiResponse<OrgUnitTreeResponseDto>.Fail($"Error retrieving organizational tree: {ex.Message}", "INTERNAL_ERROR");
        }
    }

    public async Task<ApiResponse<List<OrgUnitDto>>> GetChildrenAsync(long id)
    {
        try
        {
            var children = await _orgUnitRepository.GetChildrenAsync(id);
            var dtos = children.Select(MapToDto).ToList();

            return ApiResponse<List<OrgUnitDto>>.Ok(dtos, "Unit children retrieved successfully");
        }
        catch (Exception ex)
        {
            return ApiResponse<List<OrgUnitDto>>.Fail($"Error retrieving unit children: {ex.Message}", "INTERNAL_ERROR");
        }
    }

    public async Task<ApiResponse<MoveOrgUnitResponseDto>> MoveAsync(long id, MoveOrgUnitRequest request, string movedBy)
    {
        try
        {
            var unit = await _orgUnitRepository.GetByIdAsync(id);
            if (unit == null || unit.IsDeleted)
                return ApiResponse<MoveOrgUnitResponseDto>.Fail("Organizational unit not found", "NOT_FOUND");

            var newParent = await _orgUnitRepository.GetByIdAsync(request.NewParentId);
            if (newParent == null || newParent.IsDeleted)
                return ApiResponse<MoveOrgUnitResponseDto>.Fail("New parent unit not found", "PARENT_NOT_FOUND");

            if (!await _orgUnitRepository.CanMoveToParentAsync(id, request.NewParentId))
                return ApiResponse<MoveOrgUnitResponseDto>.Fail("Cannot move unit to its descendant", "INVALID_HIERARCHY");

            var oldParentId = unit.ParentId;
            var oldLevel = unit.Level;
            var affectedChildrenCount = await _orgUnitRepository.GetDescendantsCountAsync(id);

            unit.ParentId = request.NewParentId;
            unit.Level = newParent.Level + 1;
            unit.Order = request.NewOrder;
            unit.UpdatedAt = DateTime.UtcNow;
            unit.UpdatedBy = movedBy;

            await _orgUnitRepository.UpdateAsync(unit);
            await _orgUnitRepository.UpdateLevelsAsync(id, unit.Level);

            var response = new MoveOrgUnitResponseDto
            {
                Id = id,
                Name = unit.Name,
                NewParentId = request.NewParentId,
                NewLevel = unit.Level,
                NewOrder = request.NewOrder,
                AffectedChildrenCount = affectedChildrenCount
            };

            return ApiResponse<MoveOrgUnitResponseDto>.Ok(response, "Organizational unit moved successfully");
        }
        catch (Exception ex)
        {
            return ApiResponse<MoveOrgUnitResponseDto>.Fail($"Error moving organizational unit: {ex.Message}", "INTERNAL_ERROR");
        }
    }

    // Head of Unit Management
    public async Task<ApiResponse<AssignHeadResponseDto>> AssignHeadAsync(long id, AssignHeadRequest request, string assignedBy)
    {
        try
        {
            var unit = await _orgUnitRepository.GetByIdAsync(id);
            if (unit == null || unit.IsDeleted)
                return ApiResponse<AssignHeadResponseDto>.Fail("Organizational unit not found", "NOT_FOUND");

            var user = await _userRepository.GetByIdAsync(long.Parse(request.StaffId));
            if (user == null || user.IsDeleted)
                return ApiResponse<AssignHeadResponseDto>.Fail("Staff member not found", "STAFF_NOT_FOUND");

            // End current head if exists
            var currentHead = await _orgUnitRepository.GetCurrentHeadAsync(id);
            if (currentHead != null)
            {
                currentHead.IsActive = false;
                currentHead.EndDate = request.EffectiveDate;
                currentHead.UpdatedAt = DateTime.UtcNow;
                currentHead.UpdatedBy = assignedBy;
                await _orgUnitHeadRepository.UpdateAsync(currentHead);
            }

            var head = new OrgUnitHead
            {
                OrgUnitId = id,
                UserId = long.Parse(request.StaffId),
                Position = request.Position,
                EffectiveDate = request.EffectiveDate,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = assignedBy
            };

            await _orgUnitHeadRepository.AddAsync(head);

            var response = new AssignHeadResponseDto
            {
                UnitId = id,
                UnitName = unit.Name,
                HeadOfUnit = new HeadOfUnitDto
                {
                    Id = head.Id,
                    Name = user.FullName,
                    Position = request.Position,
                    Email = user.Email,
                    Phone = user.PhoneNumber,
                    StaffId = user.StaffId,
                    EffectiveDate = request.EffectiveDate
                }
            };

            return ApiResponse<AssignHeadResponseDto>.Ok(response, "Head of unit assigned successfully");
        }
        catch (Exception ex)
        {
            return ApiResponse<AssignHeadResponseDto>.Fail($"Error assigning head of unit: {ex.Message}", "INTERNAL_ERROR");
        }
    }

    public async Task<ApiResponse<RemoveHeadResponseDto>> RemoveHeadAsync(long id, RemoveHeadRequest request, string removedBy)
    {
        try
        {
            var unit = await _orgUnitRepository.GetByIdAsync(id);
            if (unit == null || unit.IsDeleted)
                return ApiResponse<RemoveHeadResponseDto>.Fail("Organizational unit not found", "NOT_FOUND");

            var currentHead = await _orgUnitRepository.GetCurrentHeadAsync(id);
            if (currentHead == null)
                return ApiResponse<RemoveHeadResponseDto>.Fail("No active head found for this unit", "NO_HEAD_FOUND");

            currentHead.IsActive = false;
            currentHead.EndDate = request.EffectiveDate;
            currentHead.Reason = request.Reason;
            currentHead.UpdatedAt = DateTime.UtcNow;
            currentHead.UpdatedBy = removedBy;

            await _orgUnitHeadRepository.UpdateAsync(currentHead);

            var response = new RemoveHeadResponseDto
            {
                UnitId = id,
                UnitName = unit.Name,
                RemovedHead = new HeadOfUnitDto
                {
                    Id = currentHead.Id,
                    Name = currentHead.User?.FullName ?? string.Empty,
                    Position = currentHead.Position,
                    EffectiveDate = currentHead.EffectiveDate,
                    EndDate = request.EffectiveDate
                }
            };

            return ApiResponse<RemoveHeadResponseDto>.Ok(response, "Head of unit removed successfully");
        }
        catch (Exception ex)
        {
            return ApiResponse<RemoveHeadResponseDto>.Fail($"Error removing head of unit: {ex.Message}", "INTERNAL_ERROR");
        }
    }

    public async Task<ApiResponse<List<HeadOfUnitHistoryDto>>> GetHeadHistoryAsync(long id)
    {
        try
        {
            var history = await _orgUnitRepository.GetHeadHistoryAsync(id);
            var dtos = history.Select(MapHeadHistoryToDto).ToList();

            return ApiResponse<List<HeadOfUnitHistoryDto>>.Ok(dtos, "Head of unit history retrieved successfully");
        }
        catch (Exception ex)
        {
            return ApiResponse<List<HeadOfUnitHistoryDto>>.Fail($"Error retrieving head history: {ex.Message}", "INTERNAL_ERROR");
        }
    }

    // Statistics and Metrics
    public async Task<ApiResponse<OrgUnitStatisticsDto>> GetStatisticsAsync(string? type = null)
    {
        try
        {
            var totalUnits = await _orgUnitRepository.GetTotalUnitsCountAsync(type);
            var activeUnits = await _orgUnitRepository.GetActiveUnitsCountAsync(type);
            var inactiveUnits = await _orgUnitRepository.GetInactiveUnitsCountAsync(type);
            var byType = await _orgUnitRepository.GetUnitsByTypeAsync();
            var byLevel = await _orgUnitRepository.GetUnitsByLevelAsync();

            // Calculate staff and posts counts
            var units = await _orgUnitRepository.GetAllWithDetailsAsync();
            if (!string.IsNullOrEmpty(type))
                units = units.Where(u => u.Type.ToString() == type);

            var totalStaff = units.Sum(u => u.Posts.Sum(p => p.PostOccupancies.Count(po => po.IsCurrentlyOccupying)));
            var totalPosts = units.Sum(u => u.Posts.Count);
            var vacantPosts = totalPosts - totalStaff;

            var statistics = new OrgUnitStatisticsDto
            {
                Overview = new OrgUnitOverviewDto
                {
                    TotalUnits = totalUnits,
                    ActiveUnits = activeUnits,
                    InactiveUnits = inactiveUnits,
                    TotalStaff = totalStaff,
                    TotalPosts = totalPosts,
                    VacantPosts = vacantPosts
                },
                ByType = byType.ToDictionary(kvp => kvp.Key, kvp => new OrgUnitTypeStatsDto
                {
                    Count = kvp.Value,
                    Staff = units.Where(u => u.Type.ToString() == kvp.Key).Sum(u => u.Posts.Sum(p => p.PostOccupancies.Count(po => po.IsCurrentlyOccupying))),
                    Posts = units.Where(u => u.Type.ToString() == kvp.Key).Sum(u => u.Posts.Count),
                    VacantPosts = units.Where(u => u.Type.ToString() == kvp.Key).Sum(u => u.Posts.Count) - 
                                 units.Where(u => u.Type.ToString() == kvp.Key).Sum(u => u.Posts.Sum(p => p.PostOccupancies.Count(po => po.IsCurrentlyOccupying)))
                }),
                ByLevel = byLevel.ToDictionary(kvp => kvp.Key.ToString(), kvp => new OrgUnitLevelStatsDto
                {
                    Units = kvp.Value,
                    Staff = units.Where(u => u.Level == kvp.Key).Sum(u => u.Posts.Sum(p => p.PostOccupancies.Count(po => po.IsCurrentlyOccupying))),
                    Posts = units.Where(u => u.Level == kvp.Key).Sum(u => u.Posts.Count)
                }),
                Hierarchy = new OrgUnitHierarchyDto
                {
                    MaxDepth = byLevel.Keys.DefaultIfEmpty(0).Max(),
                    AverageChildrenPerUnit = units.Where(u => u.Children.Any()).Count() > 0 ? 
                        (double)units.Sum(u => u.Children.Count) / units.Where(u => u.Children.Any()).Count() : 0,
                    UnitsWithChildren = units.Count(u => u.Children.Any()),
                    LeafUnits = units.Count(u => !u.Children.Any())
                }
            };

            return ApiResponse<OrgUnitStatisticsDto>.Ok(statistics, "Organizational unit statistics retrieved successfully");
        }
        catch (Exception ex)
        {
            return ApiResponse<OrgUnitStatisticsDto>.Fail($"Error retrieving statistics: {ex.Message}", "INTERNAL_ERROR");
        }
    }

    public async Task<ApiResponse<OrgUnitMetricsDto>> GetMetricsAsync(long id)
    {
        try
        {
            var unit = await _orgUnitRepository.GetWithDetailsAsync(id);
            if (unit == null)
                return ApiResponse<OrgUnitMetricsDto>.Fail("Organizational unit not found", "NOT_FOUND");

            var staffCount = await _orgUnitRepository.GetStaffCountAsync(id);
            var postsCount = await _orgUnitRepository.GetPostsCountAsync(id);
            var vacantPostsCount = await _orgUnitRepository.GetVacantPostsCountAsync(id);
            var childrenCount = await _orgUnitRepository.GetChildrenCountAsync(id);

            var metrics = new OrgUnitMetricsDto
            {
                UnitId = id,
                UnitName = unit.Name,
                Metrics = new OrgUnitMetricsDataDto
                {
                    Staffing = new OrgUnitStaffingDto
                    {
                        TotalStaff = staffCount,
                        ActiveStaff = staffCount, // Assuming all staff are active
                        InactiveStaff = 0,
                        StaffTurnoverRate = 0, // Would need historical data
                        AverageTenure = 0 // Would need historical data
                    },
                    Posts = new OrgUnitPostsDto
                    {
                        TotalPosts = postsCount,
                        OccupiedPosts = postsCount - vacantPostsCount,
                        VacantPosts = vacantPostsCount,
                        OccupancyRate = postsCount > 0 ? (double)(postsCount - vacantPostsCount) / postsCount * 100 : 0
                    },
                    Performance = new OrgUnitPerformanceDto
                    {
                        AverageAppraisalScore = 0, // Would need appraisal data
                        CompletedAppraisals = 0,
                        PendingAppraisals = 0,
                        CompletionRate = 0
                    },
                    Hierarchy = new OrgUnitHierarchyMetricsDto
                    {
                        ChildrenCount = childrenCount,
                        MaxDepth = await _orgUnitRepository.GetMaxLevelAsync(),
                        AverageChildrenPerChild = childrenCount > 0 ? (double)childrenCount / childrenCount : 0
                    }
                },
                Trends = new OrgUnitTrendsDto
                {
                    StaffGrowth = new OrgUnitGrowthDto
                    {
                        LastMonth = 0, // Would need historical data
                        LastQuarter = 0,
                        LastYear = 0
                    },
                    PerformanceTrend = new OrgUnitPerformanceTrendDto
                    {
                        LastMonth = 0,
                        LastQuarter = 0,
                        LastYear = 0
                    }
                }
            };

            return ApiResponse<OrgUnitMetricsDto>.Ok(metrics, "Unit performance metrics retrieved successfully");
        }
        catch (Exception ex)
        {
            return ApiResponse<OrgUnitMetricsDto>.Fail($"Error retrieving metrics: {ex.Message}", "INTERNAL_ERROR");
        }
    }

    // Bulk Operations
    public async Task<ApiResponse<BulkOperationResultDto>> BulkCreateAsync(BulkCreateOrgUnitsRequest request, string createdBy)
    {
        try
        {
            var successful = new List<BulkOperationItemDto>();
            var errors = new List<BulkOperationItemDto>();
            var index = 0;

            foreach (var unitRequest in request.Units)
            {
                try
                {
                    var result = await CreateAsync(unitRequest, createdBy);
                    if (result.Success)
                    {
                        successful.Add(new BulkOperationItemDto
                        {
                            Index = index,
                            Success = true,
                            UnitId = result.Data!.Id,
                            Name = result.Data.Name
                        });
                    }
                    else
                    {
                        errors.Add(new BulkOperationItemDto
                        {
                            Index = index,
                            Success = false,
                            Error = result.Message
                        });
                    }
                }
                catch (Exception ex)
                {
                    errors.Add(new BulkOperationItemDto
                    {
                        Index = index,
                        Success = false,
                        Error = ex.Message
                    });
                }
                index++;
            }

            var response = new BulkOperationResultDto
            {
                Created = successful.Count,
                Failed = errors.Count,
                Results = successful.Concat(errors).OrderBy(r => r.Index).ToList()
            };

            return ApiResponse<BulkOperationResultDto>.Ok(response, "Bulk creation completed");
        }
        catch (Exception ex)
        {
            return ApiResponse<BulkOperationResultDto>.Fail($"Error in bulk creation: {ex.Message}", "INTERNAL_ERROR");
        }
    }

    public async Task<ApiResponse<BulkOperationResultDto>> BulkUpdateAsync(BulkUpdateOrgUnitsRequest request, string updatedBy)
    {
        try
        {
            var successful = new List<BulkOperationItemDto>();
            var errors = new List<BulkOperationItemDto>();
            var index = 0;

            foreach (var update in request.Updates)
            {
                try
                {
                    var updateRequest = new UpdateOrgUnitRequest
                    {
                        Name = update.Name,
                        Description = update.Description,
                        Order = update.Order
                    };

                    var result = await UpdateAsync(update.Id, updateRequest, updatedBy);
                    if (result.Success)
                    {
                        successful.Add(new BulkOperationItemDto
                        {
                            Index = index,
                            Success = true,
                            UnitId = result.Data!.Id,
                            Name = result.Data.Name
                        });
                    }
                    else
                    {
                        errors.Add(new BulkOperationItemDto
                        {
                            Index = index,
                            Success = false,
                            Error = result.Message
                        });
                    }
                }
                catch (Exception ex)
                {
                    errors.Add(new BulkOperationItemDto
                    {
                        Index = index,
                        Success = false,
                        Error = ex.Message
                    });
                }
                index++;
            }

            var response = new BulkOperationResultDto
            {
                Updated = successful.Count,
                Failed = errors.Count,
                Results = successful.Concat(errors).OrderBy(r => r.Index).ToList()
            };

            return ApiResponse<BulkOperationResultDto>.Ok(response, "Bulk update completed");
        }
        catch (Exception ex)
        {
            return ApiResponse<BulkOperationResultDto>.Fail($"Error in bulk update: {ex.Message}", "INTERNAL_ERROR");
        }
    }

    // Export Operations
    public async Task<IActionResult> ExportAsync(string format = "csv", string? type = null, bool includeInactive = false)
    {
        try
        {
            var result = await GetAllAsync(1, int.MaxValue, null, type, null, includeInactive ? null : "active");
            
            if (!result.Success)
            {
                return null;
            }

            var csv = "Id,Name,Type,Description,ParentId,Level,Order,Status,ChildrenCount,PostsCount,StaffCount,DateCreated\n";
            foreach (var unit in result.Data!.Units)
            {
                csv += $"{unit.Id},{unit.Name},{unit.Type},{unit.Description},{unit.ParentId},{unit.Level},{unit.Order},{unit.Status},{unit.ChildrenCount},{unit.PostsCount},{unit.StaffCount},{unit.DateCreated:yyyy-MM-dd}\n";
            }

            var bytes = System.Text.Encoding.UTF8.GetBytes(csv);
            return new FileContentResult(bytes, "text/csv")
            {
                FileDownloadName = $"organizational-units-{DateTime.Now:yyyy-MM-dd}.csv"
            };
        }
        catch (Exception ex)
        {
            return new BadRequestObjectResult(ApiResponse<object>.Fail($"Error exporting data: {ex.Message}", "EXPORT_ERROR"));
        }
    }

    public async Task<IActionResult> ExportTreeAsync(string format = "png", int? maxDepth = null)
    {
        try
        {
            var result = await GetTreeAsync(false, maxDepth);
            
            if (!result.Success)
            {
                return null;
            }

            // For now, return a simple text representation
            // In a real implementation, you would generate an actual image
            var treeText = BuildTreeText(result.Data!.Tree, 0);
            var bytes = System.Text.Encoding.UTF8.GetBytes(treeText);
            
            return new FileContentResult(bytes, "text/plain")
            {
                FileDownloadName = $"organizational-tree-{DateTime.Now:yyyy-MM-dd}.txt"
            };
        }
        catch (Exception ex)
        {
            return new BadRequestObjectResult(ApiResponse<object>.Fail($"Error exporting tree: {ex.Message}", "EXPORT_ERROR"));
        }
    }

    // Validation
    public async Task<ApiResponse<ValidationResultDto>> ValidateAsync(ValidateOrgUnitRequest request)
    {
        try
        {
            var validation = new ValidationResultDto
            {
                Valid = true,
                Warnings = new List<string>(),
                Errors = new List<string>(),
                Suggestions = new List<ValidationSuggestionDto>()
            };

            // Check name availability
            if (await _orgUnitRepository.NameExistsAsync(request.Name, request.ParentId))
            {
                validation.Valid = false;
                validation.Errors.Add("Unit name already exists in this parent");
            }


            // Check parent exists if specified
            if (request.ParentId.HasValue)
            {
                var parent = await _orgUnitRepository.GetByIdAsync(request.ParentId.Value);
                if (parent == null || parent.IsDeleted)
                {
                    validation.Valid = false;
                    validation.Errors.Add("Parent unit not found");
                }
            }

            // Add suggestions
            if (request.Name.Contains("Dept", StringComparison.OrdinalIgnoreCase))
            {
                validation.Suggestions.Add(new ValidationSuggestionDto
                {
                    Field = "name",
                    Message = "Consider using 'Department' instead of 'Dept' for consistency"
                });
            }

            return ApiResponse<ValidationResultDto>.Ok(validation, "Validation completed");
        }
        catch (Exception ex)
        {
            return ApiResponse<ValidationResultDto>.Fail($"Error validating unit: {ex.Message}", "INTERNAL_ERROR");
        }
    }

    public async Task<ApiResponse<NameAvailabilityDto>> CheckNameAvailabilityAsync(string name, long? parentId = null, long? excludeId = null)
    {
        try
        {
            var available = !await _orgUnitRepository.NameExistsAsync(name, parentId, excludeId);
            
            var suggestions = new List<string>();
            if (!available)
            {
                // Generate suggestions based on similar names
                suggestions.Add($"{name} (Alternative)");
                suggestions.Add($"{name} Department");
                suggestions.Add($"{name} Division");
            }

            var response = new NameAvailabilityDto
            {
                Available = available,
                Suggestions = suggestions
            };

            return ApiResponse<NameAvailabilityDto>.Ok(response, "Name availability checked");
        }
        catch (Exception ex)
        {
            return ApiResponse<NameAvailabilityDto>.Fail($"Error checking name availability: {ex.Message}", "INTERNAL_ERROR");
        }
    }

    // Helper methods
    private static OrgUnitDto MapToDto(OrgUnit unit)
    {
        var currentHead = unit.HeadHistory?.FirstOrDefault(h => h.IsActive);
        
        return new OrgUnitDto
        {
            Id = unit.Id,
            Name = unit.Name,
            Type = unit.Type.ToString(),
            Description = unit.Description ?? string.Empty,
            ParentId = unit.ParentId,
            Level = unit.Level,
            Order = unit.Order,
            Status = unit.IsActive ? "active" : "inactive",
            HeadOfUnit = currentHead != null ? new HeadOfUnitDto
            {
                Id = currentHead.Id,
                Name = currentHead.User?.FullName ?? string.Empty,
                Position = currentHead.Position,
                Email = currentHead.User?.Email,
                Phone = currentHead.User?.PhoneNumber,
                StaffId = currentHead.User?.StaffId,
                EffectiveDate = currentHead.EffectiveDate,
                EndDate = currentHead.EndDate
            } : null,
            ChildrenCount = unit.Children?.Count ?? 0,
            PostsCount = unit.Posts?.Count ?? 0,
            StaffCount = unit.Posts?.Sum(p => p.PostOccupancies?.Count(po => po.IsCurrentlyOccupying) ?? 0) ?? 0,
            DateCreated = unit.CreatedAt,
            LastUpdated = unit.UpdatedAt ?? unit.CreatedAt
        };
    }

    private static HeadOfUnitHistoryDto MapHeadHistoryToDto(OrgUnitHead head)
    {
        return new HeadOfUnitHistoryDto
        {
            Id = head.Id,
            StaffId = head.User?.StaffId ?? string.Empty,
            StaffName = head.User?.FullName ?? string.Empty,
            Position = head.Position,
            EffectiveDate = head.EffectiveDate,
            EndDate = head.EndDate,
            Status = head.IsActive ? "active" : "ended",
            Reason = head.Reason
        };
    }

    private static OrgUnitTreeDto BuildTree(OrgUnit unit)
    {
        return new OrgUnitTreeDto
        {
            Id = unit.Id,
            Name = unit.Name,
            Type = unit.Type.ToString(),
            Level = unit.Level,
            Order = unit.Order,
            Children = unit.Children?.Select(BuildTree).ToList() ?? new List<OrgUnitTreeDto>()
        };
    }

    private static string BuildTreeText(List<OrgUnitTreeDto> tree, int indent)
    {
        var result = new System.Text.StringBuilder();
        var prefix = new string(' ', indent * 2);
        
        foreach (var node in tree)
        {
            result.AppendLine($"{prefix}- {node.Name} ({node.Type})");
            if (node.Children.Any())
            {
                result.Append(BuildTreeText(node.Children, indent + 1));
            }
        }
        
        return result.ToString();
    }
}
