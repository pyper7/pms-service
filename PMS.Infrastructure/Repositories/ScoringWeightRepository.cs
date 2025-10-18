using Microsoft.EntityFrameworkCore;
using PMS.Application.Interfaces;
using PMS.Application.Models.AppraisalSettings;
using PMS.Domain.Entities;
using PMS.Infrastructure.Data;
using System.Linq.Expressions;

namespace PMS.Infrastructure.Repositories;

public class ScoringWeightRepository : IScoringWeightRepository
{
    private readonly PmsDbContext _context;

    public ScoringWeightRepository(PmsDbContext context)
    {
        _context = context;
    }

    public async Task<ScoringWeight?> GetByIdAsync(long id)
    {
        return await _context.ScoringWeights
            .Where(s => s.Id == id && !s.IsDeleted)
            .FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<ScoringWeight>> GetAllAsync()
    {
        return await _context.ScoringWeights
            .Where(s => !s.IsDeleted)
            .OrderBy(s => s.Section)
            .ToListAsync();
    }

    public async Task<ScoringWeight> AddAsync(ScoringWeight entity)
    {
        _context.ScoringWeights.Add(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task UpdateAsync(ScoringWeight entity)
    {
        _context.ScoringWeights.Update(entity);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> DeleteAsync(long id)
    {
        var scoringWeight = await GetByIdAsync(id);
        if (scoringWeight == null) return false;

        scoringWeight.IsDeleted = true;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<ScoringWeightsResponse> GetScoringWeightsAsync()
    {
        var weights = await _context.ScoringWeights
            .Where(s => !s.IsDeleted)
            .OrderBy(s => s.Section)
            .Select(s => new ScoringWeightDto
            {
                Id = s.Id,
                Section = s.Section,
                Weight = s.Weight,
                Description = s.Description,
                UpdatedAt = s.UpdatedAt
            })
            .ToListAsync();

        var totalWeight = weights.Sum(w => w.Weight);
        var isValid = totalWeight == 100;

        return new ScoringWeightsResponse
        {
            Weights = weights,
            TotalWeight = totalWeight,
            IsValid = isValid
        };
    }

    public async Task<ScoringWeight?> GetBySectionAsync(string section)
    {
        return await _context.ScoringWeights
            .Where(s => s.Section == section && !s.IsDeleted)
            .FirstOrDefaultAsync();
    }

    public async Task<bool> UpdateWeightsAsync(List<ScoringWeightItem> weights, string updatedBy)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            foreach (var weightItem in weights)
            {
                var existingWeight = await GetBySectionAsync(weightItem.Section);
                if (existingWeight != null)
                {
                    existingWeight.Weight = weightItem.Weight;
                    existingWeight.UpdatedBy = updatedBy;
                    existingWeight.UpdatedAt = DateTime.UtcNow;
                    _context.ScoringWeights.Update(existingWeight);
                }
                else
                {
                    var newWeight = new ScoringWeight
                    {
                        Section = weightItem.Section,
                        Weight = weightItem.Weight,
                        CreatedBy = updatedBy
                    };
                    _context.ScoringWeights.Add(newWeight);
                }
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
            return true;
        }
        catch
        {
            await transaction.RollbackAsync();
            return false;
        }
    }

    public async Task<bool> ValidateWeightsAsync(List<ScoringWeightItem> weights)
    {
        var totalWeight = weights.Sum(w => w.Weight);
        return totalWeight == 100;
    }

    public async Task DeleteAsync(ScoringWeight entity)
    {
        entity.IsDeleted = true;
        _context.ScoringWeights.Update(entity);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<ScoringWeight>> FindAsync(Expression<Func<ScoringWeight, bool>> predicate)
    {
        return await _context.ScoringWeights
            .Where(predicate)
            .Where(s => !s.IsDeleted)
            .ToListAsync();
    }

    public async Task<ScoringWeight?> FirstOrDefaultAsync(Expression<Func<ScoringWeight, bool>> predicate)
    {
        return await _context.ScoringWeights
            .Where(predicate)
            .Where(s => !s.IsDeleted)
            .FirstOrDefaultAsync();
    }

    public async Task<bool> ExistsAsync(Expression<Func<ScoringWeight, bool>> predicate)
    {
        return await _context.ScoringWeights
            .Where(predicate)
            .Where(s => !s.IsDeleted)
            .AnyAsync();
    }

    public async Task<int> CountAsync(Expression<Func<ScoringWeight, bool>>? predicate = null)
    {
        var query = _context.ScoringWeights.Where(s => !s.IsDeleted);
        
        if (predicate != null)
        {
            query = query.Where(predicate);
        }
        
        return await query.CountAsync();
    }
}
