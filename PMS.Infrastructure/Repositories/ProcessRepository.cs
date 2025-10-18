using Microsoft.EntityFrameworkCore;
using PMS.Application.Interfaces;
using PMS.Application.Models.AppraisalSettings;
using PMS.Domain.Entities;
using PMS.Infrastructure.Data;
using System.Linq.Expressions;

namespace PMS.Infrastructure.Repositories;

public class ProcessRepository : IProcessRepository
{
    private readonly PmsDbContext _context;

    public ProcessRepository(PmsDbContext context)
    {
        _context = context;
    }

    public async Task<Process?> GetByIdAsync(long id)
    {
        return await _context.Processes
            .Where(p => p.Id == id && !p.IsDeleted)
            .FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<Process>> GetAllAsync()
    {
        return await _context.Processes
            .Where(p => !p.IsDeleted)
            .OrderBy(p => p.Name)
            .ToListAsync();
    }

    public async Task<Process> AddAsync(Process entity)
    {
        _context.Processes.Add(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task UpdateAsync(Process entity)
    {
        _context.Processes.Update(entity);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> DeleteAsync(long id)
    {
        var process = await GetByIdAsync(id);
        if (process == null) return false;

        process.IsDeleted = true;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<ProcessListResponse> GetProcessesAsync(ProcessFilters filters, int page, int pageSize)
    {
        var query = _context.Processes
            .Where(p => !p.IsDeleted);

        // Apply filters
        if (!string.IsNullOrEmpty(filters.Search))
        {
            query = query.Where(p => p.Name.Contains(filters.Search) || 
                                   (p.Description != null && p.Description.Contains(filters.Search)));
        }

        if (filters.IsActive.HasValue)
        {
            query = query.Where(p => p.IsActive == filters.IsActive.Value);
        }

        var totalItems = await query.CountAsync();
        var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

        var processes = await query
            .OrderBy(p => p.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new ProcessDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                Weight = p.Weight,
                IsActive = p.IsActive,
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt
            })
            .ToListAsync();

        return new ProcessListResponse
        {
            Processes = processes,
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

    public async Task<Process?> GetByNameAsync(string name)
    {
        return await _context.Processes
            .Where(p => p.Name == name && !p.IsDeleted)
            .FirstOrDefaultAsync();
    }

    public async Task<List<Process>> GetActiveProcessesAsync()
    {
        return await _context.Processes
            .Where(p => !p.IsDeleted && p.IsActive)
            .OrderBy(p => p.Name)
            .ToListAsync();
    }

    public async Task<bool> ExistsByNameAsync(string name, long? excludeId = null)
    {
        var query = _context.Processes
            .Where(p => p.Name == name && !p.IsDeleted);

        if (excludeId.HasValue)
        {
            query = query.Where(p => p.Id != excludeId.Value);
        }

        return await query.AnyAsync();
    }

    public async Task DeleteAsync(Process entity)
    {
        entity.IsDeleted = true;
        _context.Processes.Update(entity);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<Process>> FindAsync(Expression<Func<Process, bool>> predicate)
    {
        return await _context.Processes
            .Where(predicate)
            .Where(p => !p.IsDeleted)
            .ToListAsync();
    }

    public async Task<Process?> FirstOrDefaultAsync(Expression<Func<Process, bool>> predicate)
    {
        return await _context.Processes
            .Where(predicate)
            .Where(p => !p.IsDeleted)
            .FirstOrDefaultAsync();
    }

    public async Task<bool> ExistsAsync(Expression<Func<Process, bool>> predicate)
    {
        return await _context.Processes
            .Where(predicate)
            .Where(p => !p.IsDeleted)
            .AnyAsync();
    }

    public async Task<int> CountAsync(Expression<Func<Process, bool>>? predicate = null)
    {
        var query = _context.Processes.Where(p => !p.IsDeleted);
        
        if (predicate != null)
        {
            query = query.Where(predicate);
        }
        
        return await query.CountAsync();
    }
}
