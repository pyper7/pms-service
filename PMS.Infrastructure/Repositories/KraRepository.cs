using Microsoft.EntityFrameworkCore;
using PMS.Application.Interfaces;
using PMS.Domain.Entities;
using PMS.Infrastructure.Data;

namespace PMS.Infrastructure.Repositories;

public class KraRepository : Repository<Kra>, IKraRepository
{
    public KraRepository(PmsDbContext context) : base(context)
    {
    }

    public async Task<Kra?> GetByIdWithChildrenAsync(long id)
    {
        return await _context.Kras
            .Include(k => k.AssignedOrgUnits)
            .Include(k => k.Objectives)
                .ThenInclude(o => o.AssignedOrgUnits)
            .Include(k => k.Objectives)
                .ThenInclude(o => o.AssignedWeights)
            .Include(k => k.Objectives)
                .ThenInclude(o => o.Kpis)
            .FirstOrDefaultAsync(k => k.Id == id && !k.IsDeleted);
    }

    public Task<IQueryable<Kra>> GetByPeriodAsync(string workingYearId, string appraisalPeriodId)
    {
        IQueryable<Kra> query = _context.Kras
            .Include(k => k.AssignedOrgUnits)
            .Where(k => k.WorkingYearId == workingYearId && k.AppraisalPeriodId == appraisalPeriodId && !k.IsDeleted);
        return Task.FromResult(query);
    }

    public async Task<int> GetTotalWeightForPeriodAsync(string workingYearId, string appraisalPeriodId, long? excludeKraId = null)
    {
        var query = _context.Kras.Where(k => k.WorkingYearId == workingYearId && k.AppraisalPeriodId == appraisalPeriodId && !k.IsDeleted);
        if (excludeKraId.HasValue)
        {
            query = query.Where(k => k.Id != excludeKraId.Value);
        }
        return await query.SumAsync(k => (int?)k.Weight) ?? 0;
    }
}


