using PMS.Application.Interfaces;
using PMS.Application.Models;
using PMS.Domain.Entities;

namespace PMS.Application.Services;

public class ObjectiveService : IObjectiveService
{
    private readonly IKraRepository _kraRepository;
    private readonly IRepository<Objective> _objectiveRepo;
    private readonly IRepository<ObjectiveAssignedWeight> _assignedWeightRepo;
    private readonly IRepository<Kpi> _kpiRepo;

    public ObjectiveService(IKraRepository kraRepository,
                            IRepository<Objective> objectiveRepo,
                            IRepository<ObjectiveAssignedWeight> assignedWeightRepo,
                            IRepository<Kpi> kpiRepo)
    {
        _kraRepository = kraRepository;
        _objectiveRepo = objectiveRepo;
        _assignedWeightRepo = assignedWeightRepo;
        _kpiRepo = kpiRepo;
    }

    public async Task<ApiResponse<Objective>> CreateAsync(long kraId, Objective input, string createdBy)
    {
        var kra = await _kraRepository.GetByIdWithChildrenAsync(kraId);
        if (kra == null) return ApiResponse<Objective>.Fail("KRA not found", "NOT_FOUND");

        var currentTotal = (kra.Objectives?.Where(o => !o.IsDeleted).Sum(o => o.Weight) ?? 0);
        if (currentTotal + input.Weight > kra.Weight)
            return ApiResponse<Objective>.Fail("Objective weights exceed KRA weight", "VALIDATION_ERROR");

        input.KraId = kraId;
        input.CreatedAt = DateTime.UtcNow;
        input.CreatedBy = createdBy;
        await _objectiveRepo.AddAsync(input);
        return ApiResponse<Objective>.Ok(input, "Objective created");
    }

    public async Task<ApiResponse<List<Objective>>> ListByKraAsync(long kraId)
    {
        var kra = await _kraRepository.GetByIdWithChildrenAsync(kraId);
        if (kra == null) return ApiResponse<List<Objective>>.Fail("KRA not found", "NOT_FOUND");
        var items = kra.Objectives?.Where(o => !o.IsDeleted).ToList() ?? new List<Objective>();
        return ApiResponse<List<Objective>>.Ok(items);
    }

    public async Task<ApiResponse<Objective>> GetByIdAsync(long id)
    {
        var obj = await _objectiveRepo.GetByIdAsync(id);
        if (obj == null || obj.IsDeleted) return ApiResponse<Objective>.Fail("Objective not found", "NOT_FOUND");
        return ApiResponse<Objective>.Ok(obj);
    }

    public async Task<ApiResponse<Objective>> UpdateAsync(long id, Objective input, string updatedBy)
    {
        var obj = await _objectiveRepo.GetByIdAsync(id);
        if (obj == null || obj.IsDeleted) return ApiResponse<Objective>.Fail("Objective not found", "NOT_FOUND");

        if (input.Weight != 0 && input.Weight != obj.Weight)
        {
            var kra = await _kraRepository.GetByIdWithChildrenAsync(obj.KraId);
            var totalExcluding = kra!.Objectives!.Where(o => !o.IsDeleted && o.Id != id).Sum(o => o.Weight);
            if (totalExcluding + input.Weight > kra.Weight)
                return ApiResponse<Objective>.Fail("Objective weights exceed KRA weight", "VALIDATION_ERROR");
            obj.Weight = input.Weight;
        }

        if (!string.IsNullOrWhiteSpace(input.Title)) obj.Title = input.Title;
        obj.Description = input.Description;
        obj.UpdatedAt = DateTime.UtcNow;
        obj.UpdatedBy = updatedBy;
        await _objectiveRepo.UpdateAsync(obj);
        return ApiResponse<Objective>.Ok(obj, "Objective updated");
    }

    public async Task<ApiResponse<object>> DeleteAsync(long id, bool force, string deletedBy)
    {
        var obj = await _objectiveRepo.GetByIdAsync(id);
        if (obj == null || obj.IsDeleted) return ApiResponse<object>.Fail("Objective not found", "NOT_FOUND");

        // If not force and has KPIs
        if (!force)
        {
            var kpiCount = (await _kpiRepo.GetAllAsync()).Count(k => k.ObjectiveId == id && !k.IsDeleted);
            if (kpiCount > 0) return ApiResponse<object>.Fail("Objective has KPIs; use force=true", "CONFLICT");
        }

        obj.IsDeleted = true; obj.UpdatedAt = DateTime.UtcNow; obj.UpdatedBy = deletedBy;
        await _objectiveRepo.UpdateAsync(obj);
        return ApiResponse.Ok("Objective deleted");
    }

    public async Task<ApiResponse<object>> ValidateObjectiveWeightsAsync(long kraId)
    {
        var kra = await _kraRepository.GetByIdWithChildrenAsync(kraId);
        if (kra == null) return ApiResponse<object>.Fail("KRA not found", "NOT_FOUND");
        var total = kra.Objectives?.Where(o => !o.IsDeleted).Sum(o => o.Weight) ?? 0;
        var ok = total == kra.Weight;
        return ApiResponse<object>.Ok(new { ok, message = ok ? "Objective weights total matches KRA" : $"Total {total} vs KRA {kra.Weight}", kraWeight = kra.Weight, objectiveTotal = total, details = kra.Objectives?.Select(o => new { objectiveId = o.Id, title = o.Title, weight = o.Weight }).ToList() });
    }

    public async Task<ApiResponse<Objective>> UpdateAssignedWeightsAsync(long objectiveId, List<(long unitId, string scope, int weight)> weights, string updatedBy)
    {
        var obj = await _objectiveRepo.GetByIdAsync(objectiveId);
        if (obj == null || obj.IsDeleted) return ApiResponse<Objective>.Fail("Objective not found", "NOT_FOUND");

        // Remove existing
        var existing = (await _assignedWeightRepo.GetAllAsync()).Where(w => w.ObjectiveId == objectiveId && !w.IsDeleted).ToList();
        foreach (var e in existing)
        {
            e.IsDeleted = true; e.UpdatedAt = DateTime.UtcNow; e.UpdatedBy = updatedBy;
            await _assignedWeightRepo.UpdateAsync(e);
        }

        foreach (var w in weights)
        {
            await _assignedWeightRepo.AddAsync(new ObjectiveAssignedWeight
            {
                ObjectiveId = objectiveId,
                UnitId = w.unitId,
                Scope = w.scope,
                Weight = w.weight,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = updatedBy
            });
        }

        obj.UpdatedAt = DateTime.UtcNow;
        obj.UpdatedBy = updatedBy;
        await _objectiveRepo.UpdateAsync(obj);
        return ApiResponse<Objective>.Ok(obj, "Assigned weights updated");
    }

    public async Task<ApiResponse<object>> ValidateAssignedWeightsAsync(long objectiveId)
    {
        var obj = await _objectiveRepo.GetByIdAsync(objectiveId);
        if (obj == null || obj.IsDeleted) return ApiResponse<object>.Fail("Objective not found", "NOT_FOUND");
        var weights = (await _assignedWeightRepo.GetAllAsync()).Where(w => w.ObjectiveId == objectiveId && !w.IsDeleted).ToList();
        var total = weights.Sum(w => w.Weight);
        var ok = total == obj.Weight;
        return ApiResponse<object>.Ok(new { ok, message = ok ? "Assigned weights equal objective weight" : $"Assigned {total} vs Objective {obj.Weight}", objectiveWeight = obj.Weight, assignedTotal = total, details = weights.Select(w => new { unitId = w.UnitId, scope = w.Scope, weight = w.Weight }).ToList() });
    }
}


