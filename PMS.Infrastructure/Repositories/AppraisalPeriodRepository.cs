using Microsoft.EntityFrameworkCore;
using PMS.Application.Interfaces;
using PMS.Application.Models.AppraisalSettings;
using PMS.Domain.Entities;
using PMS.Infrastructure.Data;
using System.Linq.Expressions;

namespace PMS.Infrastructure.Repositories;

public class AppraisalPeriodRepository : IAppraisalPeriodRepository
{
    private readonly PmsDbContext _context;

    public AppraisalPeriodRepository(PmsDbContext context)
    {
        _context = context;
    }

    public async Task<AppraisalPeriod?> GetByIdAsync(long id)
    {
        return await _context.AppraisalPeriods
            .Where(p => p.Id == id && !p.IsDeleted)
            .FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<AppraisalPeriod>> GetAllAsync()
    {
        return await _context.AppraisalPeriods
            .Where(p => !p.IsDeleted)
            .OrderBy(p => p.StartDate)
            .ToListAsync();
    }

    public async Task<AppraisalPeriod> AddAsync(AppraisalPeriod entity)
    {
        _context.AppraisalPeriods.Add(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task UpdateAsync(AppraisalPeriod entity)
    {
        _context.AppraisalPeriods.Update(entity);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> DeleteAsync(long id)
    {
        var period = await GetByIdAsync(id);
        if (period == null) return false;

        period.IsDeleted = true;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<PeriodListResponse> GetPeriodsAsync(PeriodFilters filters, int page, int pageSize)
    {
        var query = _context.AppraisalPeriods
            .Where(p => !p.IsDeleted);

        // Apply filters
        if (!string.IsNullOrEmpty(filters.Search))
        {
            query = query.Where(p => p.Name.Contains(filters.Search));
        }

        if (filters.IsActive.HasValue)
        {
            query = query.Where(p => p.IsActive == filters.IsActive.Value);
        }

        if (filters.Year.HasValue)
        {
            query = query.Where(p => p.StartDate.Year == filters.Year.Value);
        }

        var totalItems = await query.CountAsync();
        var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

        var periods = await query
            .OrderBy(p => p.StartDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new AppraisalPeriodDto
            {
                Id = p.Id,
                Name = p.Name,
                StartDate = p.StartDate,
                EndDate = p.EndDate,
                IsActive = p.IsActive,
                CreatedAt = p.CreatedAt
            })
            .ToListAsync();

        return new PeriodListResponse
        {
            Periods = periods,
            Pagination = new PaginationInfo
            {
                CurrentPage = page,
                PageSize = pageSize,
                TotalPages = totalPages,
                TotalItems = totalItems,
                HasNext = page < totalPages,
                HasPrevious = page > 1
            }
        };
    }

    public async Task<AppraisalPeriod?> GetByNameAsync(string name)
    {
        return await _context.AppraisalPeriods
            .Where(p => p.Name == name && !p.IsDeleted)
            .FirstOrDefaultAsync();
    }

    public async Task<List<AppraisalPeriod>> GetActivePeriodsAsync()
    {
        return await _context.AppraisalPeriods
            .Where(p => !p.IsDeleted && p.IsActive)
            .OrderBy(p => p.StartDate)
            .ToListAsync();
    }

    public async Task<List<AppraisalPeriod>> GetByYearAsync(int year)
    {
        return await _context.AppraisalPeriods
            .Where(p => !p.IsDeleted && p.StartDate.Year == year)
            .OrderBy(p => p.StartDate)
            .ToListAsync();
    }

    public async Task<bool> ExistsByNameAsync(string name, long? excludeId = null)
    {
        var query = _context.AppraisalPeriods
            .Where(p => p.Name == name && !p.IsDeleted);

        if (excludeId.HasValue)
        {
            query = query.Where(p => p.Id != excludeId.Value);
        }

        return await query.AnyAsync();
    }

    public async Task<bool> HasOverlappingPeriodAsync(DateTime startDate, DateTime endDate, long? excludeId = null)
    {
        var query = _context.AppraisalPeriods
            .Where(p => !p.IsDeleted && 
                       ((p.StartDate <= startDate && p.EndDate >= startDate) ||
                        (p.StartDate <= endDate && p.EndDate >= endDate) ||
                        (p.StartDate >= startDate && p.EndDate <= endDate)));

        if (excludeId.HasValue)
        {
            query = query.Where(p => p.Id != excludeId.Value);
        }

        return await query.AnyAsync();
    }

    public async Task DeleteAsync(AppraisalPeriod entity)
    {
        entity.IsDeleted = true;
        _context.AppraisalPeriods.Update(entity);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<AppraisalPeriod>> FindAsync(Expression<Func<AppraisalPeriod, bool>> predicate)
    {
        return await _context.AppraisalPeriods
            .Where(predicate)
            .Where(p => !p.IsDeleted)
            .ToListAsync();
    }

    public async Task<AppraisalPeriod?> FirstOrDefaultAsync(Expression<Func<AppraisalPeriod, bool>> predicate)
    {
        return await _context.AppraisalPeriods
            .Where(predicate)
            .Where(p => !p.IsDeleted)
            .FirstOrDefaultAsync();
    }

    public async Task<bool> ExistsAsync(Expression<Func<AppraisalPeriod, bool>> predicate)
    {
        return await _context.AppraisalPeriods
            .Where(predicate)
            .Where(p => !p.IsDeleted)
            .AnyAsync();
    }

    public async Task<int> CountAsync(Expression<Func<AppraisalPeriod, bool>>? predicate = null)
    {
        var query = _context.AppraisalPeriods.Where(p => !p.IsDeleted);
        
        if (predicate != null)
        {
            query = query.Where(predicate);
        }
        
        return await query.CountAsync();
    }
}
