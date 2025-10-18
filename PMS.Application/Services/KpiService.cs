using PMS.Application.Interfaces;
using PMS.Application.Models;
using PMS.Domain.Entities;

namespace PMS.Application.Services;

public class KpiService : IKpiService
{
    private readonly IRepository<Kpi> _kpiRepo;
    private readonly IRepository<Objective> _objectiveRepo;

    public KpiService(IRepository<Kpi> kpiRepo, IRepository<Objective> objectiveRepo)
    {
        _kpiRepo = kpiRepo;
        _objectiveRepo = objectiveRepo;
    }

    public async Task<ApiResponse<Kpi>> CreateAsync(long objectiveId, Kpi input, string createdBy)
    {
        var objective = await _objectiveRepo.GetByIdAsync(objectiveId);
        if (objective == null || objective.IsDeleted) return ApiResponse<Kpi>.Fail("Objective not found", "NOT_FOUND");

        var currentTotal = (await _kpiRepo.GetAllAsync()).Where(k => k.ObjectiveId == objectiveId && !k.IsDeleted).Sum(k => k.Weight);
        if (currentTotal + input.Weight > objective.Weight)
            return ApiResponse<Kpi>.Fail("KPI weights exceed Objective weight", "VALIDATION_ERROR");

        input.ObjectiveId = objectiveId;
        input.CreatedAt = DateTime.UtcNow;
        input.CreatedBy = createdBy;
        await _kpiRepo.AddAsync(input);
        return ApiResponse<Kpi>.Ok(input, "KPI created");
    }

    public async Task<ApiResponse<List<Kpi>>> ListByObjectiveAsync(long objectiveId)
    {
        var objective = await _objectiveRepo.GetByIdAsync(objectiveId);
        if (objective == null || objective.IsDeleted) return ApiResponse<List<Kpi>>.Fail("Objective not found", "NOT_FOUND");
        var items = (await _kpiRepo.GetAllAsync()).Where(k => k.ObjectiveId == objectiveId && !k.IsDeleted).ToList();
        return ApiResponse<List<Kpi>>.Ok(items);
    }

    public async Task<ApiResponse<Kpi>> GetByIdAsync(long id)
    {
        var kpi = await _kpiRepo.GetByIdAsync(id);
        if (kpi == null || kpi.IsDeleted) return ApiResponse<Kpi>.Fail("KPI not found", "NOT_FOUND");
        return ApiResponse<Kpi>.Ok(kpi);
    }

    public async Task<ApiResponse<Kpi>> UpdateAsync(long id, Kpi input, string updatedBy)
    {
        var kpi = await _kpiRepo.GetByIdAsync(id);
        if (kpi == null || kpi.IsDeleted) return ApiResponse<Kpi>.Fail("KPI not found", "NOT_FOUND");

        if (input.Weight != 0 && input.Weight != kpi.Weight)
        {
            var objective = await _objectiveRepo.GetByIdAsync(kpi.ObjectiveId);
            var totalExcluding = (await _kpiRepo.GetAllAsync()).Where(k => k.ObjectiveId == kpi.ObjectiveId && !k.IsDeleted && k.Id != id).Sum(k => k.Weight);
            if (totalExcluding + input.Weight > objective!.Weight)
                return ApiResponse<Kpi>.Fail("KPI weights exceed Objective weight", "VALIDATION_ERROR");
            kpi.Weight = input.Weight;
        }

        if (!string.IsNullOrWhiteSpace(input.Name)) kpi.Name = input.Name;
        kpi.Description = input.Description;
        kpi.Target = input.Target;
        kpi.Unit = input.Unit;
        kpi.MeasurementType = input.MeasurementType;
        kpi.DataSource = input.DataSource;
        kpi.UpdatedAt = DateTime.UtcNow;
        kpi.UpdatedBy = updatedBy;
        await _kpiRepo.UpdateAsync(kpi);
        return ApiResponse<Kpi>.Ok(kpi, "KPI updated");
    }

    public async Task<ApiResponse<object>> DeleteAsync(long id, string deletedBy)
    {
        var kpi = await _kpiRepo.GetByIdAsync(id);
        if (kpi == null || kpi.IsDeleted) return ApiResponse<object>.Fail("KPI not found", "NOT_FOUND");
        kpi.IsDeleted = true; kpi.UpdatedAt = DateTime.UtcNow; kpi.UpdatedBy = deletedBy;
        await _kpiRepo.UpdateAsync(kpi);
        return ApiResponse.Ok("KPI deleted");
    }

    public async Task<ApiResponse<object>> ValidateKpiWeightsAsync(long objectiveId)
    {
        var objective = await _objectiveRepo.GetByIdAsync(objectiveId);
        if (objective == null || objective.IsDeleted) return ApiResponse<object>.Fail("Objective not found", "NOT_FOUND");
        var total = (await _kpiRepo.GetAllAsync()).Where(k => k.ObjectiveId == objectiveId && !k.IsDeleted).Sum(k => k.Weight);
        var ok = total == objective.Weight;
        return ApiResponse<object>.Ok(new { ok, message = ok ? "KPI weights equal objective weight" : $"KPI total {total} vs Objective {objective.Weight}", objectiveWeight = objective.Weight, kpiTotal = total, details = (await _kpiRepo.GetAllAsync()).Where(k => k.ObjectiveId == objectiveId && !k.IsDeleted).Select(k => new { kpiId = k.Id, name = k.Name, weight = k.Weight }).ToList() });
    }
}


