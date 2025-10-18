using Microsoft.EntityFrameworkCore;
using PMS.Application.Interfaces;
using PMS.Application.Models.Officers;
using PMS.Domain.Entities;
using PMS.Infrastructure.Data;

namespace PMS.Infrastructure.Repositories;

public class OfficerRepository : IOfficerRepository
{
    private readonly PmsDbContext _context;

    public OfficerRepository(PmsDbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetByIdAsync(long id)
    {
        return await _context.Users.FindAsync(id);
    }

    public async Task<User?> GetByIdWithDetailsAsync(long id)
    {
        return await _context.Users
            .Include(u => u.Qualifications)
            .Include(u => u.WorkHistory)
            .Include(u => u.StatusHistory)
            .Include(u => u.PostOccupancies)
                .ThenInclude(po => po.Post)
            .FirstOrDefaultAsync(u => u.Id == id);
    }

    public async Task<IEnumerable<User>> GetAllAsync()
    {
        return await _context.Users.ToListAsync();
    }

    public async Task<IEnumerable<User>> GetAllWithDetailsAsync()
    {
        return await _context.Users
            .Include(u => u.Qualifications)
            .Include(u => u.WorkHistory)
            .Include(u => u.StatusHistory)
            .Include(u => u.PostOccupancies)
                .ThenInclude(po => po.Post)
            .ToListAsync();
    }

    public async Task<User> CreateAsync(User user)
    {
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        return user;
    }

    public async Task<User> UpdateAsync(User user)
    {
        _context.Users.Update(user);
        await _context.SaveChangesAsync();
        return user;
    }

    public async Task DeleteAsync(long id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user != null)
        {
            user.IsDeleted = true;
            user.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }

    public async Task<(IEnumerable<User> users, int total)> SearchAsync(
        int page, int limit, string? search, string? department, 
        string? gradeLevel, string? status, string? position,
        string? sortBy, string? sortOrder)
    {
        var query = _context.Users.AsQueryable();

        // Apply filters
        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(u => 
                u.FirstName.Contains(search) || 
                u.LastName.Contains(search) || 
                u.StaffId!.Contains(search) || 
                u.Email.Contains(search) || 
                u.Department!.Contains(search));
        }

        if (!string.IsNullOrEmpty(department))
            query = query.Where(u => u.Department == department);

        if (!string.IsNullOrEmpty(gradeLevel))
            query = query.Where(u => u.GradeLevel == gradeLevel);

        if (!string.IsNullOrEmpty(status))
            query = query.Where(u => u.Status == status);

        if (!string.IsNullOrEmpty(position))
            query = query.Where(u => u.Position!.Contains(position));

        // Get total count
        var total = await query.CountAsync();

        // Apply sorting
        query = sortBy.ToLower() switch
        {
            "name" => sortOrder.ToLower() == "desc" ? query.OrderByDescending(u => u.FirstName) : query.OrderBy(u => u.FirstName),
            "staffid" => sortOrder.ToLower() == "desc" ? query.OrderByDescending(u => u.StaffId) : query.OrderBy(u => u.StaffId),
            "department" => sortOrder.ToLower() == "desc" ? query.OrderByDescending(u => u.Department) : query.OrderBy(u => u.Department),
            "gradelevel" => sortOrder.ToLower() == "desc" ? query.OrderByDescending(u => u.GradeLevel) : query.OrderBy(u => u.GradeLevel),
            "datecreated" => sortOrder.ToLower() == "desc" ? query.OrderByDescending(u => u.CreatedAt) : query.OrderBy(u => u.CreatedAt),
            _ => query.OrderBy(u => u.FirstName)
        };

        // Apply pagination
        var users = await query
            .Skip((page - 1) * limit)
            .Take(limit)
            .Include(u => u.Qualifications)
            .Include(u => u.WorkHistory)
            .Include(u => u.PostOccupancies)
                .ThenInclude(po => po.Post)
            .ToListAsync();

        return (users, total);
    }

    public async Task<(IEnumerable<User> users, int total)> AdvancedSearchAsync(SearchCriteriaDto criteria, int page, int limit, string? sortBy, string? sortOrder)
    {
        var query = _context.Users.AsQueryable();

        if (criteria != null)
        {
            if (!string.IsNullOrEmpty(criteria.Name))
            {
                query = query.Where(u => 
                    u.FirstName.Contains(criteria.Name) || 
                    u.LastName.Contains(criteria.Name));
            }

            if (!string.IsNullOrEmpty(criteria.Department))
                query = query.Where(u => u.Department == criteria.Department);

            if (criteria.GradeLevel != null)
            {
                if (criteria.GradeLevel.Min.HasValue)
                    query = query.Where(u => int.Parse(u.GradeLevel!) >= criteria.GradeLevel.Min.Value);
                if (criteria.GradeLevel.Max.HasValue)
                    query = query.Where(u => int.Parse(u.GradeLevel!) <= criteria.GradeLevel.Max.Value);
            }

            if (criteria.DateOfEmployment != null)
            {
                if (criteria.DateOfEmployment.From.HasValue)
                    query = query.Where(u => u.EmploymentDate >= criteria.DateOfEmployment.From.Value);
                if (criteria.DateOfEmployment.To.HasValue)
                    query = query.Where(u => u.EmploymentDate <= criteria.DateOfEmployment.To.Value);
            }

            if (criteria.Qualifications != null)
            {
                if (!string.IsNullOrEmpty(criteria.Qualifications.Degree))
                {
                    query = query.Where(u => u.Qualifications.Any(q => 
                        q.Degree.Contains(criteria.Qualifications.Degree)));
                }
                if (!string.IsNullOrEmpty(criteria.Qualifications.Field))
                {
                    query = query.Where(u => u.Qualifications.Any(q => 
                        q.Field.Contains(criteria.Qualifications.Field)));
                }
            }
        }

        var total = await query.CountAsync();

        // Apply sorting
        query = (sortBy?.ToLower() ?? "name") switch
        {
            "name" => (sortOrder?.ToLower() ?? "asc") == "desc" ? query.OrderByDescending(u => u.FirstName) : query.OrderBy(u => u.FirstName),
            "staffid" => (sortOrder?.ToLower() ?? "asc") == "desc" ? query.OrderByDescending(u => u.StaffId) : query.OrderBy(u => u.StaffId),
            "department" => (sortOrder?.ToLower() ?? "asc") == "desc" ? query.OrderByDescending(u => u.Department) : query.OrderBy(u => u.Department),
            "gradelevel" => (sortOrder?.ToLower() ?? "asc") == "desc" ? query.OrderByDescending(u => u.GradeLevel) : query.OrderBy(u => u.GradeLevel),
            "datecreated" => (sortOrder?.ToLower() ?? "asc") == "desc" ? query.OrderByDescending(u => u.CreatedAt) : query.OrderBy(u => u.CreatedAt),
            _ => query.OrderBy(u => u.FirstName)
        };

        var users = await query
            .Skip((page - 1) * limit)
            .Take(limit)
            .Include(u => u.Qualifications)
            .Include(u => u.WorkHistory)
            .Include(u => u.PostOccupancies)
                .ThenInclude(po => po.Post)
            .ToListAsync();

        return (users, total);
    }

    public async Task<OfficerStatisticsDto> GetStatisticsAsync(string? department, string? gradeLevel, DateTime? dateFrom, DateTime? dateTo)
    {
        var query = _context.Users.AsQueryable();

        if (!string.IsNullOrEmpty(department))
            query = query.Where(u => u.Department == department);

        if (!string.IsNullOrEmpty(gradeLevel))
            query = query.Where(u => u.GradeLevel == gradeLevel);

        if (dateFrom.HasValue)
            query = query.Where(u => u.CreatedAt >= dateFrom.Value);

        if (dateTo.HasValue)
            query = query.Where(u => u.CreatedAt <= dateTo.Value);

        var users = await query.ToListAsync();

        var statistics = new OfficerStatisticsDto
        {
            TotalOfficers = users.Count,
            ActiveOfficers = users.Count(u => !string.IsNullOrEmpty(u.Status) && u.Status == "active"),
            InactiveOfficers = users.Count(u => !string.IsNullOrEmpty(u.Status) && u.Status == "inactive"),
            ByDepartment = users.Where(u => !string.IsNullOrEmpty(u.Department))
                .GroupBy(u => u.Department!)
                .ToDictionary(g => g.Key, g => g.Count()),
            ByGradeLevel = users.Where(u => !string.IsNullOrEmpty(u.GradeLevel))
                .GroupBy(u => u.GradeLevel!)
                .ToDictionary(g => g.Key, g => g.Count()),
            ByStatus = users.Where(u => !string.IsNullOrEmpty(u.Status))
                .GroupBy(u => u.Status!)
                .ToDictionary(g => g.Key, g => g.Count()),
            AverageAge = CalculateAverageAge(users),
            AverageYearsOfService = CalculateAverageYearsOfService(users),
            NewHiresThisYear = users.Count(u => u.CreatedAt.Year == DateTime.UtcNow.Year),
            RetirementsThisYear = users.Count(u => !string.IsNullOrEmpty(u.Status) && u.Status == "inactive" && u.UpdatedAt?.Year == DateTime.UtcNow.Year)
        };

        return statistics;
    }

    public async Task<IEnumerable<Qualification>> GetQualificationsAsync(long userId)
    {
        return await _context.Qualifications
            .Where(q => q.UserId == userId)
            .OrderByDescending(q => q.Year)
            .ToListAsync();
    }

    public async Task<Qualification?> GetQualificationByIdAsync(long id)
    {
        return await _context.Qualifications.FindAsync(id);
    }

    public async Task<Qualification> CreateQualificationAsync(Qualification qualification)
    {
        _context.Qualifications.Add(qualification);
        await _context.SaveChangesAsync();
        return qualification;
    }

    public async Task<Qualification> UpdateQualificationAsync(Qualification qualification)
    {
        _context.Qualifications.Update(qualification);
        await _context.SaveChangesAsync();
        return qualification;
    }

    public async Task DeleteQualificationAsync(long id)
    {
        var qualification = await _context.Qualifications.FindAsync(id);
        if (qualification != null)
        {
            qualification.IsDeleted = true;
            qualification.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }

    public async Task<IEnumerable<WorkHistory>> GetWorkHistoryAsync(long userId)
    {
        return await _context.WorkHistories
            .Where(w => w.UserId == userId)
            .OrderByDescending(w => w.StartDate)
            .ToListAsync();
    }

    public async Task<WorkHistory?> GetWorkHistoryByIdAsync(long id)
    {
        return await _context.WorkHistories.FindAsync(id);
    }

    public async Task<WorkHistory> CreateWorkHistoryAsync(WorkHistory workHistory)
    {
        _context.WorkHistories.Add(workHistory);
        await _context.SaveChangesAsync();
        return workHistory;
    }

    public async Task<WorkHistory> UpdateWorkHistoryAsync(WorkHistory workHistory)
    {
        _context.WorkHistories.Update(workHistory);
        await _context.SaveChangesAsync();
        return workHistory;
    }

    public async Task DeleteWorkHistoryAsync(long id)
    {
        var workHistory = await _context.WorkHistories.FindAsync(id);
        if (workHistory != null)
        {
            workHistory.IsDeleted = true;
            workHistory.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }

    public async Task<IEnumerable<StatusHistory>> GetStatusHistoryAsync(long userId)
    {
        return await _context.StatusHistories
            .Where(s => s.UserId == userId)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync();
    }

    public async Task<StatusHistory> CreateStatusHistoryAsync(StatusHistory statusHistory)
    {
        _context.StatusHistories.Add(statusHistory);
        await _context.SaveChangesAsync();
        return statusHistory;
    }

    public async Task<bool> StaffIdExistsAsync(string staffId, long? excludeId = null)
    {
        var query = _context.Users.Where(u => u.StaffId == staffId);
        if (excludeId.HasValue)
            query = query.Where(u => u.Id != excludeId.Value);
        
        return await query.AnyAsync();
    }

    public async Task<bool> EmailExistsAsync(string email, long? excludeId = null)
    {
        var query = _context.Users.Where(u => u.Email == email);
        if (excludeId.HasValue)
            query = query.Where(u => u.Id != excludeId.Value);
        
        return await query.AnyAsync();
    }

    public async Task<IEnumerable<User>> BulkCreateAsync(IEnumerable<User> users)
    {
        _context.Users.AddRange(users);
        await _context.SaveChangesAsync();
        return users;
    }

    public async Task<IEnumerable<User>> BulkUpdateAsync(IEnumerable<User> users)
    {
        _context.Users.UpdateRange(users);
        await _context.SaveChangesAsync();
        return users;
    }

    public async Task BulkDeleteAsync(IEnumerable<long> ids)
    {
        var users = await _context.Users.Where(u => ids.Contains(u.Id)).ToListAsync();
        foreach (var user in users)
        {
            user.IsDeleted = true;
            user.UpdatedAt = DateTime.UtcNow;
        }
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<User>> GetForExportAsync(string? search, string? department, string? gradeLevel, string? status, bool includeQualifications, bool includeWorkHistory)
    {
        var query = _context.Users.AsQueryable();

        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(u => 
                u.FirstName.Contains(search) || 
                u.LastName.Contains(search) || 
                u.StaffId!.Contains(search) || 
                u.Email.Contains(search) || 
                u.Department!.Contains(search));
        }

        if (!string.IsNullOrEmpty(department))
            query = query.Where(u => u.Department == department);

        if (!string.IsNullOrEmpty(gradeLevel))
            query = query.Where(u => u.GradeLevel == gradeLevel);

        if (!string.IsNullOrEmpty(status))
            query = query.Where(u => u.Status == status);

        if (includeQualifications)
            query = query.Include(u => u.Qualifications);

        if (includeWorkHistory)
            query = query.Include(u => u.WorkHistory);

        return await query.ToListAsync();
    }

    private static double CalculateAverageAge(List<User> users)
    {
        var usersWithBirthDate = users.Where(u => u.DateOfBirth.HasValue).ToList();
        if (!usersWithBirthDate.Any())
            return 0;

        return usersWithBirthDate.Average(u => DateTime.UtcNow.Year - u.DateOfBirth!.Value.Year);
    }

    private static double CalculateAverageYearsOfService(List<User> users)
    {
        var usersWithEmploymentDate = users.Where(u => u.EmploymentDate.HasValue).ToList();
        if (!usersWithEmploymentDate.Any())
            return 0;

        return usersWithEmploymentDate.Average(u => DateTime.UtcNow.Year - u.EmploymentDate!.Value.Year);
    }
}