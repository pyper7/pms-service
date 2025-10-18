using PMS.Application.Models.AppraisalSettings;
using PMS.Domain.Entities;

namespace PMS.Application.Interfaces;

public interface IProcessRepository : IRepository<Process>
{
    Task<ProcessListResponse> GetProcessesAsync(ProcessFilters filters, int page, int pageSize);
    Task<Process?> GetByNameAsync(string name);
    Task<List<Process>> GetActiveProcessesAsync();
    Task<bool> ExistsByNameAsync(string name, long? excludeId = null);
}
