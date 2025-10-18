using PMS.Domain.Entities;

namespace PMS.Application.Interfaces;

public interface IKraRepository : IRepository<Kra>
{
    Task<Kra?> GetByIdWithChildrenAsync(long id);
    Task<IQueryable<Kra>> GetByPeriodAsync(string workingYearId, string appraisalPeriodId);
    Task<int> GetTotalWeightForPeriodAsync(string workingYearId, string appraisalPeriodId, long? excludeKraId = null);
}


