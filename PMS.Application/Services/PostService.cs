using Microsoft.AspNetCore.Mvc;
using PMS.Application.Interfaces;
using PMS.Application.Models;
using PMS.Application.Models.Posts;
using PMS.Domain.Entities;

namespace PMS.Application.Services;

public class PostService : IPostService
{
    private readonly IPostRepository _postRepository;
    private readonly IRepository<OrgUnit> _orgUnitRepository;
    private readonly IRepository<Role> _roleRepository;
    private readonly IRepository<User> _userRepository;
    private readonly IRepository<PostOccupancy> _postOccupancyRepository;

    public PostService(
        IPostRepository postRepository,
        IRepository<OrgUnit> orgUnitRepository,
        IRepository<Role> roleRepository,
        IRepository<User> userRepository,
        IRepository<PostOccupancy> postOccupancyRepository)
    {
        _postRepository = postRepository;
        _orgUnitRepository = orgUnitRepository;
        _roleRepository = roleRepository;
        _userRepository = userRepository;
        _postOccupancyRepository = postOccupancyRepository;
    }

    // Post Management
    public async Task<ApiResponse<PostListDto>> GetAllAsync(int page = 1, int limit = 10, string? search = null, long? orgUnitId = null, string? status = null, bool? isOccupied = null, string? gradeLevel = null, string? sortBy = "name", string? sortOrder = "asc")
    {
        try
        {
            var posts = await _postRepository.GetAllWithDetailsAsync();
            
            // Apply filters
            if (!string.IsNullOrWhiteSpace(search))
            {
                posts = posts.Where(p => p.Title.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                                       p.OrgUnit?.Name.Contains(search, StringComparison.OrdinalIgnoreCase) == true);
            }
            
            
            if (orgUnitId.HasValue)
            {
                posts = posts.Where(p => p.OrgUnitId == orgUnitId.Value);
            }
            
            if (!string.IsNullOrWhiteSpace(status))
            {
                posts = posts.Where(p => p.Status.ToString().Equals(status, StringComparison.OrdinalIgnoreCase));
            }
            
            if (isOccupied.HasValue)
            {
                if (isOccupied.Value)
                {
                    posts = posts.Where(p => p.PostOccupancies.Any(po => po.IsCurrentlyOccupying));
                }
                else
                {
                    posts = posts.Where(p => !p.PostOccupancies.Any(po => po.IsCurrentlyOccupying));
                }
            }
            
            if (!string.IsNullOrWhiteSpace(gradeLevel))
            {
                posts = posts.Where(p => p.GradeLevel.Contains(gradeLevel, StringComparison.OrdinalIgnoreCase));
            }

            // Apply sorting
            posts = sortBy.ToLower() switch
            {
                "name" => sortOrder.ToLower() == "desc" ? posts.OrderByDescending(p => p.Title) : posts.OrderBy(p => p.Title),
                "datecreated" => sortOrder.ToLower() == "desc" ? posts.OrderByDescending(p => p.CreatedAt) : posts.OrderBy(p => p.CreatedAt),
                "lastupdated" => sortOrder.ToLower() == "desc" ? posts.OrderByDescending(p => p.UpdatedAt) : posts.OrderBy(p => p.UpdatedAt),
                _ => posts.OrderBy(p => p.Title)
            };

            var total = posts.Count();
            var totalPages = (int)Math.Ceiling((double)total / limit);
            var skip = (page - 1) * limit;
            
            var pagedPosts = posts.Skip(skip).Take(limit).ToList();
            var postDtos = pagedPosts.Select(MapToDto).ToList();

            var pagination = new PaginationDto
            {
                Total = total,
                Page = page,
                Limit = limit,
                TotalPages = totalPages,
                HasNext = page < totalPages,
                HasPrevious = page > 1
            };

            var result = new PostListDto
            {
                Posts = postDtos,
                Pagination = pagination
            };

            return ApiResponse<PostListDto>.Ok(result, "Posts retrieved successfully");
        }
        catch (Exception ex)
        {
            return ApiResponse<PostListDto>.Fail($"Error retrieving posts: {ex.Message}", "INTERNAL_ERROR");
        }
    }

    public async Task<ApiResponse<PostDto>> GetByIdAsync(long id)
    {
        try
        {
            var post = await _postRepository.GetWithDetailsAsync(id);
            if (post == null || post.IsDeleted)
                return ApiResponse<PostDto>.Fail("Post not found", "POST_NOT_FOUND");

            var dto = MapToDto(post);
            return ApiResponse<PostDto>.Ok(dto, "Post retrieved successfully");
        }
        catch (Exception ex)
        {
            return ApiResponse<PostDto>.Fail($"Error retrieving post: {ex.Message}", "INTERNAL_ERROR");
        }
    }

    public async Task<ApiResponse<PostDto>> CreateAsync(CreatePostRequest request, string createdBy)
    {
        try
        {
            if (await _postRepository.TitleExistsAsync(request.Name))
                return ApiResponse<PostDto>.Fail("Post name already exists", "DUPLICATE_NAME");

            // Validate related entities exist
            var orgUnit = await _orgUnitRepository.GetByIdAsync(request.OrgUnitId);
            if (orgUnit == null || orgUnit.IsDeleted)
                return ApiResponse<PostDto>.Fail("Organization unit not found", "ORG_UNIT_NOT_FOUND");

            if (request.RoleId.HasValue)
            {
                var role = await _roleRepository.GetByIdAsync(request.RoleId.Value);
                if (role == null || role.IsDeleted)
                    return ApiResponse<PostDto>.Fail("Role not found", "ROLE_NOT_FOUND");
            }

            var post = new Post
            {
                Title = request.Name,
                GradeLevel = request.GradeLevel,
                IsUnique = true,
                OrgUnitId = request.OrgUnitId,
                RoleId = request.RoleId ?? 0,
                Status = Enum.Parse<PostStatus>(request.Status, true),
                CreatedAt = DateTime.UtcNow,
                CreatedBy = createdBy
            };

            await _postRepository.AddAsync(post);
            return await GetByIdAsync(post.Id);
        }
        catch (Exception ex)
        {
            return ApiResponse<PostDto>.Fail($"Error creating post: {ex.Message}", "INTERNAL_ERROR");
        }
    }

    public async Task<ApiResponse<PostDto>> UpdateAsync(long id, UpdatePostRequest request, string updatedBy)
    {
        try
        {
            var post = await _postRepository.GetByIdAsync(id);
            if (post == null || post.IsDeleted)
                return ApiResponse<PostDto>.Fail("Post not found", "POST_NOT_FOUND");

            if (await _postRepository.TitleExistsAsync(request.Name, id))
                return ApiResponse<PostDto>.Fail("Post name already exists", "DUPLICATE_NAME");

            post.Title = request.Name;
            post.GradeLevel = request.GradeLevel;
            post.Status = Enum.Parse<PostStatus>(request.Status, true);
            post.UpdatedAt = DateTime.UtcNow;
            post.UpdatedBy = updatedBy;

            await _postRepository.UpdateAsync(post);
            return await GetByIdAsync(id);
        }
        catch (Exception ex)
        {
            return ApiResponse<PostDto>.Fail($"Error updating post: {ex.Message}", "INTERNAL_ERROR");
        }
    }

    public async Task<ApiResponse<object>> DeleteAsync(long id, string deletedBy)
    {
        try
        {
            var post = await _postRepository.GetByIdAsync(id);
            if (post == null || post.IsDeleted)
                return ApiResponse<object>.Fail("Post not found", "POST_NOT_FOUND");

            post.IsDeleted = true;
            post.UpdatedAt = DateTime.UtcNow;
            post.UpdatedBy = deletedBy;
            await _postRepository.UpdateAsync(post);
            return ApiResponse.Ok("Post deleted successfully");
        }
        catch (Exception ex)
        {
            return ApiResponse<object>.Fail($"Error deleting post: {ex.Message}", "INTERNAL_ERROR");
        }
    }

    // Role Management
    public async Task<ApiResponse<object>> AssignRoleAsync(long postId, long roleId, string assignedBy)
    {
        try
        {
            var post = await _postRepository.GetByIdAsync(postId);
            if (post == null || post.IsDeleted)
                return ApiResponse<object>.Fail("Post not found", "POST_NOT_FOUND");

            var role = await _roleRepository.GetByIdAsync(roleId);
            if (role == null || role.IsDeleted)
                return ApiResponse<object>.Fail("Role not found", "ROLE_NOT_FOUND");

            post.RoleId = roleId;
            post.UpdatedAt = DateTime.UtcNow;
            post.UpdatedBy = assignedBy;
            await _postRepository.UpdateAsync(post);

            return ApiResponse.Ok("Role assigned to post successfully");
        }
        catch (Exception ex)
        {
            return ApiResponse<object>.Fail($"Error assigning role: {ex.Message}", "INTERNAL_ERROR");
        }
    }

    public async Task<ApiResponse<object>> RemoveRoleAsync(long postId, string removedBy)
    {
        try
        {
            var post = await _postRepository.GetByIdAsync(postId);
            if (post == null || post.IsDeleted)
                return ApiResponse<object>.Fail("Post not found", "POST_NOT_FOUND");

            post.RoleId = 0;
            post.UpdatedAt = DateTime.UtcNow;
            post.UpdatedBy = removedBy;
            await _postRepository.UpdateAsync(post);

            return ApiResponse.Ok("Role removed from post successfully");
        }
        catch (Exception ex)
        {
            return ApiResponse<object>.Fail($"Error removing role: {ex.Message}", "INTERNAL_ERROR");
        }
    }

    // Post Occupancy Management
    public async Task<ApiResponse<PostOccupancyListDto>> GetAllOccupanciesAsync(int page = 1, int limit = 10, string? search = null, long? postId = null, long? officerId = null, bool? isActive = null, DateTime? startDate = null, DateTime? endDate = null, string? sortBy = "startDate", string? sortOrder = "desc")
    {
        try
        {
            var occupancies = await _postRepository.GetAllOccupanciesWithDetailsAsync();
            
            // Apply filters
            if (!string.IsNullOrWhiteSpace(search))
            {
                occupancies = occupancies.Where(o => o.User?.FullName.Contains(search, StringComparison.OrdinalIgnoreCase) == true ||
                                                   o.Post?.Title.Contains(search, StringComparison.OrdinalIgnoreCase) == true);
            }
            
            if (postId.HasValue)
            {
                occupancies = occupancies.Where(o => o.PostId == postId.Value);
            }
            
            if (officerId.HasValue)
            {
                occupancies = occupancies.Where(o => o.UserId == officerId.Value);
            }
            
            if (isActive.HasValue)
            {
                occupancies = occupancies.Where(o => o.IsCurrentlyOccupying == isActive.Value);
            }
            
            if (startDate.HasValue)
            {
                occupancies = occupancies.Where(o => o.StartDate >= startDate.Value);
            }
            
            if (endDate.HasValue)
            {
                occupancies = occupancies.Where(o => o.StartDate <= endDate.Value);
            }

            // Apply sorting
            occupancies = sortBy.ToLower() switch
            {
                "startdate" => sortOrder.ToLower() == "desc" ? occupancies.OrderByDescending(o => o.StartDate) : occupancies.OrderBy(o => o.StartDate),
                "enddate" => sortOrder.ToLower() == "desc" ? occupancies.OrderByDescending(o => o.EndDate) : occupancies.OrderBy(o => o.EndDate),
                "officername" => sortOrder.ToLower() == "desc" ? occupancies.OrderByDescending(o => o.User!.FullName) : occupancies.OrderBy(o => o.User!.FullName),
                _ => occupancies.OrderByDescending(o => o.StartDate)
            };

            var total = occupancies.Count();
            var totalPages = (int)Math.Ceiling((double)total / limit);
            var skip = (page - 1) * limit;
            
            var pagedOccupancies = occupancies.Skip(skip).Take(limit).ToList();
            var occupancyDtos = pagedOccupancies.Select(MapOccupancyToDto).ToList();

            var pagination = new PaginationDto
            {
                Total = total,
                Page = page,
                Limit = limit,
                TotalPages = totalPages,
                HasNext = page < totalPages,
                HasPrevious = page > 1
            };

            var result = new PostOccupancyListDto
            {
                Occupancies = occupancyDtos,
                Pagination = pagination
            };

            return ApiResponse<PostOccupancyListDto>.Ok(result, "Post occupancies retrieved successfully");
        }
        catch (Exception ex)
        {
            return ApiResponse<PostOccupancyListDto>.Fail($"Error retrieving occupancies: {ex.Message}", "INTERNAL_ERROR");
        }
    }

    public async Task<ApiResponse<PostOccupancyDto>> GetOccupancyByIdAsync(long id)
    {
        try
        {
            var occupancy = await _postRepository.GetOccupancyWithDetailsAsync(id);
            if (occupancy == null || occupancy.IsDeleted)
                return ApiResponse<PostOccupancyDto>.Fail("Post occupancy not found", "OCCUPANCY_NOT_FOUND");

            var dto = MapOccupancyToDto(occupancy);
            return ApiResponse<PostOccupancyDto>.Ok(dto, "Post occupancy retrieved successfully");
        }
        catch (Exception ex)
        {
            return ApiResponse<PostOccupancyDto>.Fail($"Error retrieving occupancy: {ex.Message}", "INTERNAL_ERROR");
        }
    }

    public async Task<ApiResponse<PostOccupancyDto>> CreateOccupancyAsync(CreatePostOccupancyRequest request, string createdBy)
    {
        try
        {
            var post = await _postRepository.GetByIdAsync(request.PostId);
            if (post == null || post.IsDeleted)
                return ApiResponse<PostOccupancyDto>.Fail("Post not found", "POST_NOT_FOUND");

            var user = await _userRepository.GetByIdAsync(request.OfficerId);
            if (user == null || user.IsDeleted)
                return ApiResponse<PostOccupancyDto>.Fail("Officer not found", "OFFICER_NOT_FOUND");

            // Check if officer is already assigned to this post
            var existingOccupancy = await _postRepository.GetActiveOccupancyByUserAsync(request.PostId, request.OfficerId);
            if (existingOccupancy != null)
                return ApiResponse<PostOccupancyDto>.Fail("Officer is already assigned to this post", "OFFICER_ALREADY_ASSIGNED");

            var occupancy = new PostOccupancy
            {
                PostId = request.PostId,
                UserId = request.OfficerId,
                StartDate = DateTime.UtcNow,
                EndDate = null, // This makes IsCurrentlyOccupying = true
                Remarks = request.Notes,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = createdBy
            };

            await _postOccupancyRepository.AddAsync(occupancy);

            // Update user flags to reflect onboarding completion
            if (!user.IsActive || !user.IsPostAssigned)
            {
                user.IsActive = true;
                user.IsPostAssigned = true;
                user.UpdatedAt = DateTime.UtcNow;
                user.UpdatedBy = createdBy;
                await _userRepository.UpdateAsync(user);
            }

            return await GetOccupancyByIdAsync(occupancy.Id);
        }
        catch (Exception ex)
        {
            return ApiResponse<PostOccupancyDto>.Fail($"Error creating occupancy: {ex.Message}", "INTERNAL_ERROR");
        }
    }

    public async Task<ApiResponse<PostOccupancyDto>> UpdateOccupancyAsync(long id, UpdatePostOccupancyRequest request, string updatedBy)
    {
        try
        {
            var occupancy = await _postOccupancyRepository.GetByIdAsync(id);
            if (occupancy == null || occupancy.IsDeleted)
                return ApiResponse<PostOccupancyDto>.Fail("Post occupancy not found", "OCCUPANCY_NOT_FOUND");

            occupancy.Remarks = request.Notes;
            if (!request.IsActive)
            {
                occupancy.EndDate = DateTime.UtcNow; // This makes IsCurrentlyOccupying = false
            }
            occupancy.UpdatedAt = DateTime.UtcNow;
            occupancy.UpdatedBy = updatedBy;

            await _postOccupancyRepository.UpdateAsync(occupancy);
            return await GetOccupancyByIdAsync(id);
        }
        catch (Exception ex)
        {
            return ApiResponse<PostOccupancyDto>.Fail($"Error updating occupancy: {ex.Message}", "INTERNAL_ERROR");
        }
    }

    public async Task<ApiResponse<PostOccupancyDto>> EndOccupancyAsync(long id, EndPostOccupancyRequest request, string endedBy)
    {
        try
        {
            var occupancy = await _postOccupancyRepository.GetByIdAsync(id);
            if (occupancy == null || occupancy.IsDeleted)
                return ApiResponse<PostOccupancyDto>.Fail("Post occupancy not found", "OCCUPANCY_NOT_FOUND");

            occupancy.EndDate = request.EndDate;
            occupancy.Remarks = request.Notes;
            occupancy.UpdatedAt = DateTime.UtcNow;
            occupancy.UpdatedBy = endedBy;

            await _postOccupancyRepository.UpdateAsync(occupancy);
            return await GetOccupancyByIdAsync(id);
        }
        catch (Exception ex)
        {
            return ApiResponse<PostOccupancyDto>.Fail($"Error ending occupancy: {ex.Message}", "INTERNAL_ERROR");
        }
    }

    public async Task<ApiResponse<object>> DeleteOccupancyAsync(long id, string deletedBy)
    {
        try
        {
            var occupancy = await _postOccupancyRepository.GetByIdAsync(id);
            if (occupancy == null || occupancy.IsDeleted)
                return ApiResponse<object>.Fail("Post occupancy not found", "OCCUPANCY_NOT_FOUND");

            occupancy.IsDeleted = true;
            occupancy.UpdatedAt = DateTime.UtcNow;
            occupancy.UpdatedBy = deletedBy;
            await _postOccupancyRepository.UpdateAsync(occupancy);
            return ApiResponse.Ok("Post occupancy deleted successfully");
        }
        catch (Exception ex)
        {
            return ApiResponse<object>.Fail($"Error deleting occupancy: {ex.Message}", "INTERNAL_ERROR");
        }
    }

    public async Task<ApiResponse<PostOccupancyListDto>> GetOccupancyHistoryAsync(long postId, int page = 1, int limit = 10, bool includeInactive = true)
    {
        try
        {
            var occupancies = await _postRepository.GetOccupancyHistoryAsync(postId, includeInactive);
            
            var total = occupancies.Count();
            var totalPages = (int)Math.Ceiling((double)total / limit);
            var skip = (page - 1) * limit;
            
            var pagedOccupancies = occupancies.Skip(skip).Take(limit).ToList();
            var occupancyDtos = pagedOccupancies.Select(MapOccupancyToDto).ToList();

            var pagination = new PaginationDto
            {
                Total = total,
                Page = page,
                Limit = limit,
                TotalPages = totalPages,
                HasNext = page < totalPages,
                HasPrevious = page > 1
            };

            var result = new PostOccupancyListDto
            {
                Occupancies = occupancyDtos,
                Pagination = pagination
            };

            return ApiResponse<PostOccupancyListDto>.Ok(result, "Post occupancy history retrieved successfully");
        }
        catch (Exception ex)
        {
            return ApiResponse<PostOccupancyListDto>.Fail($"Error retrieving occupancy history: {ex.Message}", "INTERNAL_ERROR");
        }
    }

    // Validation and Availability
    public async Task<ApiResponse<AssignmentValidationDto>> ValidateAssignmentAsync(long postId, long officerId)
    {
        try
        {
            var post = await _postRepository.GetWithDetailsAsync(postId);
            if (post == null || post.IsDeleted)
                return ApiResponse<AssignmentValidationDto>.Fail("Post not found", "POST_NOT_FOUND");

            var officer = await _userRepository.GetByIdAsync(officerId);
            if (officer == null || officer.IsDeleted)
                return ApiResponse<AssignmentValidationDto>.Fail("Officer not found", "OFFICER_NOT_FOUND");

            var validation = new AssignmentValidationDto
            {
                IsValid = true,
                Conflicts = new List<string>(),
                Warnings = new List<ValidationWarningDto>()
            };

            // Check if officer is already assigned to this post
            var existingOccupancy = await _postRepository.GetActiveOccupancyByUserAsync(postId, officerId);
            if (existingOccupancy != null)
            {
                validation.IsValid = false;
                validation.Conflicts.Add("Officer is already assigned to this post");
            }

            // Check if post is already occupied
            var currentOccupancy = await _postRepository.GetCurrentOccupancyAsync(postId);
            if (currentOccupancy != null && currentOccupancy.UserId != officerId)
            {
                validation.IsValid = false;
                validation.Conflicts.Add("Post is already occupied by another officer");
            }

            // Add warnings
            if (post.RoleId > 0 && !string.IsNullOrEmpty(officer.GradeLevel))
            {
                // This is a simplified grade level check - you might want to implement more sophisticated logic
                validation.Warnings.Add(new ValidationWarningDto
                {
                    Type = "grade_level_check",
                    Message = "Verify officer's grade level matches post requirements"
                });
            }

            return ApiResponse<AssignmentValidationDto>.Ok(validation, "Assignment validation completed");
        }
        catch (Exception ex)
        {
            return ApiResponse<AssignmentValidationDto>.Fail($"Error validating assignment: {ex.Message}", "INTERNAL_ERROR");
        }
    }

    public async Task<ApiResponse<PostAvailabilityDto>> CheckAvailabilityAsync(long postId)
    {
        try
        {
            var post = await _postRepository.GetWithDetailsAsync(postId);
            if (post == null || post.IsDeleted)
                return ApiResponse<PostAvailabilityDto>.Fail("Post not found", "POST_NOT_FOUND");

            var currentOccupancy = await _postRepository.GetCurrentOccupancyAsync(postId);
            var isAvailable = currentOccupancy == null;

            var availability = new PostAvailabilityDto
            {
                IsAvailable = isAvailable,
                CurrentOccupancy = currentOccupancy != null ? MapOccupancyToDto(currentOccupancy) : null,
                CanAssign = isAvailable,
                Reason = isAvailable ? null : "Post is currently occupied"
            };

            return ApiResponse<PostAvailabilityDto>.Ok(availability, "Post availability checked");
        }
        catch (Exception ex)
        {
            return ApiResponse<PostAvailabilityDto>.Fail($"Error checking availability: {ex.Message}", "INTERNAL_ERROR");
        }
    }

    // Statistics
    public async Task<ApiResponse<PostStatisticsDto>> GetStatisticsAsync()
    {
        try
        {
            var posts = await _postRepository.GetAllWithDetailsAsync();
            var totalPosts = posts.Count();
            var activePosts = posts.Count(p => p.Status == PostStatus.Active);
            var inactivePosts = totalPosts - activePosts;
            var occupiedPosts = posts.Count(p => p.PostOccupancies.Any(po => po.IsCurrentlyOccupying));
            var vacantPosts = totalPosts - occupiedPosts;

            var postsByGradeLevel = posts
                .GroupBy(p => p.GradeLevel)
                .Select(g => new GradeLevelCountDto { GradeLevel = g.Key, Count = g.Count() })
                .ToList();

            var postsByOrgUnit = posts
                .GroupBy(p => new { p.OrgUnitId, p.OrgUnit!.Name })
                .Select(g => new OrgUnitCountDto { OrgUnitId = g.Key.OrgUnitId, OrgUnitName = g.Key.Name, Count = g.Count() })
                .ToList();

            var statistics = new PostStatisticsDto
            {
                TotalPosts = totalPosts,
                ActivePosts = activePosts,
                InactivePosts = inactivePosts,
                OccupiedPosts = occupiedPosts,
                VacantPosts = vacantPosts,
                PostsByGradeLevel = postsByGradeLevel,
                PostsByOrgUnit = postsByOrgUnit
            };

            return ApiResponse<PostStatisticsDto>.Ok(statistics, "Post statistics retrieved successfully");
        }
        catch (Exception ex)
        {
            return ApiResponse<PostStatisticsDto>.Fail($"Error retrieving statistics: {ex.Message}", "INTERNAL_ERROR");
        }
    }

    public async Task<ApiResponse<OccupancyStatisticsDto>> GetOccupancyStatisticsAsync()
    {
        try
        {
            var occupancies = await _postRepository.GetAllOccupanciesWithDetailsAsync();
            var totalAssignments = occupancies.Count();
            var activeAssignments = occupancies.Count(o => o.IsCurrentlyOccupying);
            var endedAssignments = totalAssignments - activeAssignments;

            var averageDuration = occupancies
                .Where(o => o.EndDate.HasValue)
                .Select(o => (o.EndDate!.Value - o.StartDate).Days)
                .DefaultIfEmpty(0)
                .Average();

            var assignmentsByMonth = occupancies
                .GroupBy(o => o.StartDate.ToString("yyyy-MM"))
                .Select(g => new MonthlyCountDto { Month = g.Key, Count = g.Count() })
                .OrderBy(x => x.Month)
                .ToList();

            var topOccupiedPosts = occupancies
                .GroupBy(o => new { o.PostId, o.Post!.Title })
                .Select(g => new TopOccupiedPostDto { PostId = g.Key.PostId, PostName = g.Key.Title, AssignmentCount = g.Count() })
                .OrderByDescending(x => x.AssignmentCount)
                .Take(10)
                .ToList();

            var statistics = new OccupancyStatisticsDto
            {
                TotalAssignments = totalAssignments,
                ActiveAssignments = activeAssignments,
                EndedAssignments = endedAssignments,
                AverageAssignmentDuration = (int)averageDuration,
                AssignmentsByMonth = assignmentsByMonth,
                TopOccupiedPosts = topOccupiedPosts
            };

            return ApiResponse<OccupancyStatisticsDto>.Ok(statistics, "Occupancy statistics retrieved successfully");
        }
        catch (Exception ex)
        {
            return ApiResponse<OccupancyStatisticsDto>.Fail($"Error retrieving occupancy statistics: {ex.Message}", "INTERNAL_ERROR");
        }
    }

    // Export
    public async Task<IActionResult> ExportAsync(string format = "csv", string? search = null, long? orgUnitId = null, string? status = null, bool? isOccupied = null, string? gradeLevel = null)
    {
        // This is a simplified implementation - you would typically use a library like EPPlus for Excel
        var result = await GetAllAsync(1, int.MaxValue, search, orgUnitId, status, isOccupied, gradeLevel);
        
        if (!result.Success)
        {
            return null;
        }

        // For now, return a simple CSV response
        var csv = "Id,Name,Description,GradeLevel,OrgUnitName,Status,IsOccupied,RoleName,DateCreated\n";
        foreach (var post in result.Data!.Posts)
        {
            csv += $"{post.Id},{post.Name},{post.Description},{post.GradeLevel},{post.OrgUnitName},{post.Status},{post.IsOccupied},{post.RoleName},{post.DateCreated:yyyy-MM-dd}\n";
        }

        var bytes = System.Text.Encoding.UTF8.GetBytes(csv);
        return new FileContentResult(bytes, "text/csv")
        {
            FileDownloadName = $"posts_export_{DateTime.Now:yyyy-MM-dd}.csv"
        };
    }

    public async Task<IActionResult> ExportOccupanciesAsync(string format = "csv", string? search = null, long? postId = null, long? officerId = null, bool? isActive = null, DateTime? startDate = null, DateTime? endDate = null)
    {
        var result = await GetAllOccupanciesAsync(1, int.MaxValue, search, postId, officerId, isActive, startDate, endDate);
        
        if (!result.Success)
        {
            return null;
        }

        // For now, return a simple CSV response
        var csv = "Id,PostName,OfficerName,StaffId,StartDate,EndDate,IsActive,Notes\n";
        foreach (var occupancy in result.Data!.Occupancies)
        {
            csv += $"{occupancy.Id},{occupancy.PostName},{occupancy.OfficerName},{occupancy.StaffId},{occupancy.StartDate:yyyy-MM-dd},{occupancy.EndDate:yyyy-MM-dd},{occupancy.IsActive},{occupancy.Notes}\n";
        }

        var bytes = System.Text.Encoding.UTF8.GetBytes(csv);
        return new FileContentResult(bytes, "text/csv")
        {
            FileDownloadName = $"post_occupancies_export_{DateTime.Now:yyyy-MM-dd}.csv"
        };
    }

    // Bulk Operations
    public async Task<ApiResponse<object>> BulkCreateAsync(List<CreatePostRequest> posts)
    {
        try
        {
            var successful = new List<object>();
            var errors = new List<object>();
            var index = 0;

            foreach (var postRequest in posts)
            {
                try
                {
                    var result = await CreateAsync(postRequest, "system");
                    if (result.Success)
                    {
                        successful.Add(new { Id = result.Data!.Id, Name = result.Data.Name, Status = "created" });
                    }
                    else
                    {
                        errors.Add(new { Index = index, Data = postRequest, Error = result.Message });
                    }
                }
                catch (Exception ex)
                {
                    errors.Add(new { Index = index, Data = postRequest, Error = ex.Message });
                }
                index++;
            }

            var response = new
            {
                Successful = successful,
                Errors = errors,
                Summary = new { Total = posts.Count, Successful = successful.Count, Errors = errors.Count }
            };

            return ApiResponse<object>.Ok(response, "Bulk post creation completed");
        }
        catch (Exception ex)
        {
            return ApiResponse<object>.Fail($"Error in bulk creation: {ex.Message}", "INTERNAL_ERROR");
        }
    }

    public async Task<ApiResponse<object>> BulkAssignOfficersAsync(List<CreatePostOccupancyRequest> assignments)
    {
        try
        {
            var successful = new List<object>();
            var errors = new List<object>();
            var index = 0;

            foreach (var assignment in assignments)
            {
                try
                {
                    var result = await CreateOccupancyAsync(assignment, "system");
                    if (result.Success)
                    {
                        successful.Add(new { Id = result.Data!.Id, PostId = assignment.PostId, OfficerId = assignment.OfficerId, Status = "assigned" });
                    }
                    else
                    {
                        errors.Add(new { Index = index, Data = assignment, Error = result.Message });
                    }
                }
                catch (Exception ex)
                {
                    errors.Add(new { Index = index, Data = assignment, Error = ex.Message });
                }
                index++;
            }

            var response = new
            {
                Successful = successful,
                Errors = errors,
                Summary = new { Total = assignments.Count, Successful = successful.Count, Errors = errors.Count }
            };

            return ApiResponse<object>.Ok(response, "Bulk officer assignment completed");
        }
        catch (Exception ex)
        {
            return ApiResponse<object>.Fail($"Error in bulk assignment: {ex.Message}", "INTERNAL_ERROR");
        }
    }

    private static PostDto MapToDto(Post post)
    {
        // Debug: Check what's in PostOccupancies
        var allOccupancies = post.PostOccupancies?.ToList() ?? new List<PostOccupancy>();
        var currentOccupancy = allOccupancies.FirstOrDefault(po => po.EndDate == null);
        
        // Debug logging (remove in production)
        if (post.Id == 1) // Only log for the Agency Admin post
        {
            Console.WriteLine($"Post {post.Title} - Occupancies count: {allOccupancies.Count}");
            foreach (var occ in allOccupancies)
            {
                Console.WriteLine($"  Occupancy: UserId={occ.UserId}, EndDate={occ.EndDate}, IsCurrentlyOccupying={occ.IsCurrentlyOccupying}");
            }
            Console.WriteLine($"Current occupancy: {currentOccupancy != null}");
        }
        
        return new PostDto
        {
            Id = post.Id,
            Name = post.Title,
            Description = post.Description ?? string.Empty,
            GradeLevel = post.GradeLevel,
            // MdaId and MdaName removed as Mda relationship was removed
            OrgUnitId = post.OrgUnitId,
            OrgUnitName = post.OrgUnit?.Name ?? string.Empty,
            OrgUnitType = post.OrgUnit?.Type.ToString(),
            Status = post.Status.ToString().ToLower(),
            IsOccupied = currentOccupancy != null,
            AssignedOfficerId = currentOccupancy?.UserId,
            AssignedOfficerName = currentOccupancy?.User?.FullName,
            RoleId = post.RoleId,
            RoleName = post.Role?.Name,
            DateCreated = post.CreatedAt,
            LastUpdated = post.UpdatedAt ?? post.CreatedAt
        };
    }

    private static PostOccupancyDto MapOccupancyToDto(PostOccupancy occupancy)
    {
        return new PostOccupancyDto
        {
            Id = occupancy.Id,
            PostName = occupancy.Post?.Title ?? string.Empty,
            OfficerId = occupancy.UserId,
            OfficerName = occupancy.User?.FullName ?? string.Empty,
            StaffId = occupancy.User?.StaffId,
            StartDate = occupancy.StartDate,
            EndDate = occupancy.EndDate,
            IsActive = occupancy.IsCurrentlyOccupying,
            Notes = occupancy.Remarks,
            DateCreated = occupancy.CreatedAt,
            LastUpdated = occupancy.UpdatedAt ?? occupancy.CreatedAt
        };
    }
}
