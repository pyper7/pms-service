using Microsoft.AspNetCore.Mvc;
using PMS.Application.Interfaces;
using PMS.Application.Models;
using PMS.Application.Models.Posts;
using PMS.Domain.Entities;

namespace PMS.Application.Services;

public class KraService : IKraService
{
    private readonly IKraRepository _kraRepository;
    private readonly IRepository<KraOrgUnitAssignment> _kraUnitRepo;
    private readonly IRepository<Objective> _objectiveRepo;
    private readonly IRepository<Kpi> _kpiRepo;

    public KraService(IKraRepository kraRepository,
                      IRepository<KraOrgUnitAssignment> kraUnitRepo,
                      IRepository<Objective> objectiveRepo,
                      IRepository<Kpi> kpiRepo)
    {
        _kraRepository = kraRepository;
        _kraUnitRepo = kraUnitRepo;
        _objectiveRepo = objectiveRepo;
        _kpiRepo = kpiRepo;
    }

    public async Task<ApiResponse<Kra>> CreateAsync(Kra input, string createdBy)
    {
        // Validate period weights (will be revalidated after save too)
        var currentTotal = await _kraRepository.GetTotalWeightForPeriodAsync(input.WorkingYearId, input.AppraisalPeriodId);
        if (currentTotal + input.Weight > 100)
            return ApiResponse<Kra>.Fail("KRA weights exceed 100 for the period", "VALIDATION_ERROR");

        input.CreatedAt = DateTime.UtcNow;
        input.CreatedBy = createdBy;
        await _kraRepository.AddAsync(input);
        return ApiResponse<Kra>.Ok(input, "KRA created");
    }

    public async Task<ApiResponse<Kra>> UpdateAsync(long id, Kra input, string updatedBy)
    {
        var kra = await _kraRepository.GetByIdAsync(id);
        if (kra == null || kra.IsDeleted) return ApiResponse<Kra>.Fail("KRA not found", "NOT_FOUND");

        // If weight changed, re-validate
        if (input.Weight != 0 && input.Weight != kra.Weight)
        {
            var totalExcluding = await _kraRepository.GetTotalWeightForPeriodAsync(kra.WorkingYearId, kra.AppraisalPeriodId, kra.Id);
            if (totalExcluding + input.Weight > 100)
                return ApiResponse<Kra>.Fail("KRA weights exceed 100 for the period", "VALIDATION_ERROR");
            kra.Weight = input.Weight;
        }

        if (!string.IsNullOrWhiteSpace(input.Title)) kra.Title = input.Title;
        kra.Description = input.Description;
        kra.UpdatedAt = DateTime.UtcNow;
        kra.UpdatedBy = updatedBy;
        await _kraRepository.UpdateAsync(kra);
        return ApiResponse<Kra>.Ok(kra, "KRA updated");
    }

    public async Task<ApiResponse<object>> DeleteAsync(long id, bool force, string deletedBy)
    {
        var kra = await _kraRepository.GetByIdWithChildrenAsync(id);
        if (kra == null) return ApiResponse<object>.Fail("KRA not found", "NOT_FOUND");
        if (!force && (kra.Objectives?.Any() == true))
            return ApiResponse<object>.Fail("KRA has child Objectives; use force=true", "CONFLICT");

        // Soft delete tree
        if (kra.Objectives != null)
        {
            foreach (var obj in kra.Objectives)
            {
                obj.IsDeleted = true; obj.UpdatedAt = DateTime.UtcNow; obj.UpdatedBy = deletedBy;
                if (obj.Kpis != null)
                {
                    foreach (var k in obj.Kpis)
                    {
                        k.IsDeleted = true; k.UpdatedAt = DateTime.UtcNow; k.UpdatedBy = deletedBy;
                    }
                }
            }
        }
        kra.IsDeleted = true; kra.UpdatedAt = DateTime.UtcNow; kra.UpdatedBy = deletedBy;
        await _kraRepository.UpdateAsync(kra);
        return ApiResponse.Ok("KRA deleted");
    }

    public async Task<ApiResponse<Kra>> GetByIdAsync(long id)
    {
        var kra = await _kraRepository.GetByIdWithChildrenAsync(id);
        if (kra == null) return ApiResponse<Kra>.Fail("KRA not found", "NOT_FOUND");
        return ApiResponse<Kra>.Ok(kra);
    }

    public async Task<ApiResponse<object>> ListAsync(string? workingYearId, string? appraisalPeriodId, int page, int limit, string? sortBy, string? sortOrder, string? q)
    {
        IQueryable<Kra> query;
        if (!string.IsNullOrWhiteSpace(workingYearId) && !string.IsNullOrWhiteSpace(appraisalPeriodId))
        {
            query = await _kraRepository.GetByPeriodAsync(workingYearId, appraisalPeriodId);
        }
        else
        {
            var all = await _kraRepository.GetAllAsync();
            query = all.AsQueryable();
        }

        if (!string.IsNullOrWhiteSpace(q)) query = query.Where(k => k.Title.Contains(q));

        query = (sortBy?.ToLower()) switch
        {
            "createdat" => (sortOrder?.ToLower() == "desc" ? query.OrderByDescending(k => k.CreatedAt) : query.OrderBy(k => k.CreatedAt)),
            "updatedat" => (sortOrder?.ToLower() == "desc" ? query.OrderByDescending(k => k.UpdatedAt) : query.OrderBy(k => k.UpdatedAt)),
            _ => (sortOrder?.ToLower() == "desc" ? query.OrderByDescending(k => k.Title) : query.OrderBy(k => k.Title))
        };

        var total = query.Count();
        var totalPages = (int)Math.Ceiling((double)total / limit);
        var items = query.Skip((page - 1) * limit).Take(limit).ToList();

        var pagination = new PaginationDto
        {
            Total = total,
            Page = page,
            Limit = limit,
            TotalPages = totalPages,
            HasNext = page < totalPages,
            HasPrevious = page > 1
        };

        return ApiResponse<object>.Ok(new { items, pagination });
    }

    public async Task<ApiResponse<object>> ValidateKraWeightsAsync(string workingYearId, string appraisalPeriodId)
    {
        var total = await _kraRepository.GetTotalWeightForPeriodAsync(workingYearId, appraisalPeriodId);
        var ok = total == 100;
        return ApiResponse<object>.Ok(new
        {
            ok,
            message = ok ? "KRA weights total 100" : $"KRA weights total {total}, expected 100",
            totalWeight = total,
            details = (await _kraRepository.GetByPeriodAsync(workingYearId, appraisalPeriodId)).Select(k => new { kraId = k.Id, title = k.Title, weight = k.Weight }).ToList()
        });
    }

    public async Task<ApiResponse<Kra>> AssignOrgUnitsAsync(long kraId, List<(long id, string type)> units, string updatedBy)
    {
        var kra = await _kraRepository.GetByIdWithChildrenAsync(kraId);
        if (kra == null) return ApiResponse<Kra>.Fail("KRA not found", "NOT_FOUND");

        // Replace assignments
        if (kra.AssignedOrgUnits != null)
        {
            foreach (var a in kra.AssignedOrgUnits) { a.IsDeleted = true; a.UpdatedAt = DateTime.UtcNow; a.UpdatedBy = updatedBy; }
        }

        foreach (var u in units)
        {
            await _kraUnitRepo.AddAsync(new KraOrgUnitAssignment
            {
                KraId = kraId,
                OrgUnitId = u.id,
                OrgUnitType = u.type,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = updatedBy
            });
        }

        kra.UpdatedAt = DateTime.UtcNow;
        kra.UpdatedBy = updatedBy;
        await _kraRepository.UpdateAsync(kra);
        return ApiResponse<Kra>.Ok(kra, "Assigned");
    }

    public Task<IActionResult> ExportAsync(string workingYearId, string appraisalPeriodId, string format)
    {
        // Stub: return 202 with an empty file or message; full export added later
        IActionResult result = new OkObjectResult(ApiResponse<object>.Ok(new { message = "Export not implemented yet" }));
        return Task.FromResult(result);
    }
}


