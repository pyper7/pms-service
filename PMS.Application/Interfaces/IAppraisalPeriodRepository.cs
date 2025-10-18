using PMS.Application.Models.AppraisalSettings;
using PMS.Domain.Entities;

namespace PMS.Application.Interfaces;

public interface IAppraisalPeriodRepository : IRepository<AppraisalPeriod>
{
    Task<PeriodListResponse> GetPeriodsAsync(PeriodFilters filters, int page, int pageSize);
    Task<AppraisalPeriod?> GetByNameAsync(string name);
    Task<List<AppraisalPeriod>> GetActivePeriodsAsync();
    Task<List<AppraisalPeriod>> GetByYearAsync(int year);
    Task<bool> ExistsByNameAsync(string name, long? excludeId = null);
    Task<bool> HasOverlappingPeriodAsync(DateTime startDate, DateTime endDate, long? excludeId = null);
}
