using PMS.Application.Models.AppraisalSettings;
using PMS.Domain.Entities;

namespace PMS.Application.Interfaces;

public interface ICompetencyRepository : IRepository<Competency>
{
    Task<CompetencyListResponse> GetCompetenciesAsync(CompetencyFilters filters, int page, int pageSize);
    Task<Competency?> GetByNameAsync(string name);
    Task<List<Competency>> GetByCategoryAsync(string category);
    Task<List<Competency>> GetActiveCompetenciesAsync();
    Task<bool> ExistsByNameAsync(string name, long? excludeId = null);
    Task<BulkOperationResponse> BulkCreateAsync(List<CreateCompetencyRequest> competencies, string createdBy);
    Task<BulkOperationResponse> BulkUpdateAsync(List<BulkUpdateCompetencyItem> competencies, string updatedBy);
}
