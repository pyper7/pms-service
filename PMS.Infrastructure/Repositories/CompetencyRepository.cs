using Microsoft.EntityFrameworkCore;
using PMS.Application.Interfaces;
using PMS.Application.Models.AppraisalSettings;
using PMS.Domain.Entities;
using PMS.Infrastructure.Data;
using System.Linq.Expressions;

namespace PMS.Infrastructure.Repositories;

public class CompetencyRepository : ICompetencyRepository
{
    private readonly PmsDbContext _context;

    public CompetencyRepository(PmsDbContext context)
    {
        _context = context;
    }

    public async Task<Competency?> GetByIdAsync(long id)
    {
        return await _context.Competencies
            .Where(c => c.Id == id && !c.IsDeleted)
            .FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<Competency>> GetAllAsync()
    {
        return await _context.Competencies
            .Where(c => !c.IsDeleted)
            .OrderBy(c => c.Name)
            .ToListAsync();
    }

    public async Task<Competency> AddAsync(Competency entity)
    {
        _context.Competencies.Add(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task UpdateAsync(Competency entity)
    {
        _context.Competencies.Update(entity);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> DeleteAsync(long id)
    {
        var competency = await GetByIdAsync(id);
        if (competency == null) return false;

        competency.IsDeleted = true;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task DeleteAsync(Competency entity)
    {
        entity.IsDeleted = true;
        _context.Competencies.Update(entity);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<Competency>> FindAsync(Expression<Func<Competency, bool>> predicate)
    {
        return await _context.Competencies
            .Where(predicate)
            .Where(c => !c.IsDeleted)
            .ToListAsync();
    }

    public async Task<Competency?> FirstOrDefaultAsync(Expression<Func<Competency, bool>> predicate)
    {
        return await _context.Competencies
            .Where(predicate)
            .Where(c => !c.IsDeleted)
            .FirstOrDefaultAsync();
    }

    public async Task<bool> ExistsAsync(Expression<Func<Competency, bool>> predicate)
    {
        return await _context.Competencies
            .Where(predicate)
            .Where(c => !c.IsDeleted)
            .AnyAsync();
    }

    public async Task<int> CountAsync(Expression<Func<Competency, bool>>? predicate = null)
    {
        var query = _context.Competencies.Where(c => !c.IsDeleted);
        
        if (predicate != null)
        {
            query = query.Where(predicate);
        }
        
        return await query.CountAsync();
    }

    public async Task<CompetencyListResponse> GetCompetenciesAsync(CompetencyFilters filters, int page, int pageSize)
    {
        var query = _context.Competencies
            .Where(c => !c.IsDeleted);

        // Apply filters
        if (!string.IsNullOrEmpty(filters.Search))
        {
            query = query.Where(c => c.Name.Contains(filters.Search) || 
                                   (c.Description != null && c.Description.Contains(filters.Search)));
        }

        if (filters.Category != "ALL")
        {
            query = query.Where(c => c.Category == filters.Category);
        }

        if (filters.IsActive.HasValue)
        {
            query = query.Where(c => c.IsActive == filters.IsActive.Value);
        }

        var totalItems = await query.CountAsync();
        var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

        var competencies = await query
            .OrderBy(c => c.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(c => new CompetencyDto
            {
                Id = c.Id,
                Name = c.Name,
                Category = c.Category,
                Description = c.Description,
                IsActive = c.IsActive,
                CreatedAt = c.CreatedAt,
                UpdatedAt = c.UpdatedAt
            })
            .ToListAsync();

        return new CompetencyListResponse
        {
            Competencies = competencies,
            Pagination = new PaginationInfo
            {
                CurrentPage = page,
                PageSize = pageSize,
                TotalPages = totalPages,
                TotalItems = totalItems,
                HasNext = page < totalPages,
                HasPrevious = page > 1
            },
            Filters = filters
        };
    }

    public async Task<Competency?> GetByNameAsync(string name)
    {
        return await _context.Competencies
            .Where(c => c.Name == name && !c.IsDeleted)
            .FirstOrDefaultAsync();
    }

    public async Task<List<Competency>> GetByCategoryAsync(string category)
    {
        return await _context.Competencies
            .Where(c => c.Category == category && !c.IsDeleted && c.IsActive)
            .OrderBy(c => c.Name)
            .ToListAsync();
    }

    public async Task<List<Competency>> GetActiveCompetenciesAsync()
    {
        return await _context.Competencies
            .Where(c => !c.IsDeleted && c.IsActive)
            .OrderBy(c => c.Name)
            .ToListAsync();
    }

    public async Task<bool> ExistsByNameAsync(string name, long? excludeId = null)
    {
        var query = _context.Competencies
            .Where(c => c.Name == name && !c.IsDeleted);

        if (excludeId.HasValue)
        {
            query = query.Where(c => c.Id != excludeId.Value);
        }

        return await query.AnyAsync();
    }

    public async Task<BulkOperationResponse> BulkCreateAsync(List<CreateCompetencyRequest> competencies, string createdBy)
    {
        var response = new BulkOperationResponse();
        var createdCompetencies = new List<CompetencyDto>();

        foreach (var competency in competencies)
        {
            try
            {
                if (await ExistsByNameAsync(competency.Name))
                {
                    response.Errors.Add(new BulkError
                    {
                        Row = response.Created + response.Failed + 1,
                        Name = competency.Name,
                        Error = "Competency with this name already exists"
                    });
                    response.Failed++;
                    continue;
                }

                var entity = new Competency
                {
                    Name = competency.Name,
                    Category = competency.Category,
                    Description = competency.Description,
                    IsActive = competency.IsActive,
                    CreatedBy = createdBy
                };

                _context.Competencies.Add(entity);
                await _context.SaveChangesAsync();

                createdCompetencies.Add(new CompetencyDto
                {
                    Id = entity.Id,
                    Name = entity.Name,
                    Category = entity.Category,
                    Description = entity.Description,
                    IsActive = entity.IsActive,
                    CreatedAt = entity.CreatedAt,
                    UpdatedAt = entity.UpdatedAt
                });

                response.Created++;
            }
            catch (Exception ex)
            {
                response.Errors.Add(new BulkError
                {
                    Row = response.Created + response.Failed + 1,
                    Name = competency.Name,
                    Error = ex.Message
                });
                response.Failed++;
            }
        }

        response.Competencies = createdCompetencies;
        return response;
    }

    public async Task<BulkOperationResponse> BulkUpdateAsync(List<BulkUpdateCompetencyItem> competencies, string updatedBy)
    {
        var response = new BulkOperationResponse();
        var updatedCompetencies = new List<CompetencyDto>();

        foreach (var competency in competencies)
        {
            try
            {
                var entity = await GetByIdAsync(competency.Id);
                if (entity == null)
                {
                    response.Errors.Add(new BulkError
                    {
                        Row = response.Updated + response.Failed + 1,
                        Name = competency.Name,
                        Error = "Competency not found"
                    });
                    response.Failed++;
                    continue;
                }

                if (await ExistsByNameAsync(competency.Name, competency.Id))
                {
                    response.Errors.Add(new BulkError
                    {
                        Row = response.Updated + response.Failed + 1,
                        Name = competency.Name,
                        Error = "Competency with this name already exists"
                    });
                    response.Failed++;
                    continue;
                }

                entity.Name = competency.Name;
                entity.Category = competency.Category;
                entity.Description = competency.Description;
                entity.IsActive = competency.IsActive;
                entity.UpdatedBy = updatedBy;
                entity.UpdatedAt = DateTime.UtcNow;

                _context.Competencies.Update(entity);
                await _context.SaveChangesAsync();

                updatedCompetencies.Add(new CompetencyDto
                {
                    Id = entity.Id,
                    Name = entity.Name,
                    Category = entity.Category,
                    Description = entity.Description,
                    IsActive = entity.IsActive,
                    CreatedAt = entity.CreatedAt,
                    UpdatedAt = entity.UpdatedAt
                });

                response.Updated++;
            }
            catch (Exception ex)
            {
                response.Errors.Add(new BulkError
                {
                    Row = response.Updated + response.Failed + 1,
                    Name = competency.Name,
                    Error = ex.Message
                });
                response.Failed++;
            }
        }

        response.Competencies = updatedCompetencies;
        return response;
    }
}
