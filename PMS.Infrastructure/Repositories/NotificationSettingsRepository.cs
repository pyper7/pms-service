using Microsoft.EntityFrameworkCore;
using PMS.Application.Interfaces;
using PMS.Application.Models.AppraisalSettings;
using PMS.Domain.Entities;
using PMS.Infrastructure.Data;
using System.Linq.Expressions;

namespace PMS.Infrastructure.Repositories;

public class NotificationSettingsRepository : INotificationSettingsRepository
{
    private readonly PmsDbContext _context;

    public NotificationSettingsRepository(PmsDbContext context)
    {
        _context = context;
    }

    public async Task<NotificationSettings?> GetByIdAsync(long id)
    {
        return await _context.NotificationSettings
            .Where(n => n.Id == id && !n.IsDeleted)
            .FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<NotificationSettings>> GetAllAsync()
    {
        return await _context.NotificationSettings
            .Where(n => !n.IsDeleted)
            .OrderBy(n => n.CreatedAt)
            .ToListAsync();
    }

    public async Task<NotificationSettings> AddAsync(NotificationSettings entity)
    {
        _context.NotificationSettings.Add(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task UpdateAsync(NotificationSettings entity)
    {
        _context.NotificationSettings.Update(entity);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> DeleteAsync(long id)
    {
        var settings = await GetByIdAsync(id);
        if (settings == null) return false;

        settings.IsDeleted = true;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<NotificationSettingsDto> GetSettingsAsync()
    {
        var settings = await GetCurrentSettingsAsync();
        if (settings == null)
        {
            // Return default settings if none exist
            return new NotificationSettingsDto
            {
                EmailNotifications = true,
                SystemNotifications = true,
                ReminderDays = 7,
                EscalationDays = 3,
                AutoReminders = true,
                DeadlineAlerts = true,
                ApprovalNotifications = true,
                RejectionNotifications = true,
                UpdateNotifications = true
            };
        }

        return new NotificationSettingsDto
        {
            EmailNotifications = settings.EmailNotifications,
            SystemNotifications = settings.SystemNotifications,
            ReminderDays = settings.ReminderDays,
            EscalationDays = settings.EscalationDays,
            AutoReminders = settings.AutoReminders,
            DeadlineAlerts = settings.DeadlineAlerts,
            ApprovalNotifications = settings.ApprovalNotifications,
            RejectionNotifications = settings.RejectionNotifications,
            UpdateNotifications = settings.UpdateNotifications,
            UpdatedAt = settings.UpdatedAt
        };
    }

    public async Task<bool> UpdateSettingsAsync(UpdateNotificationSettingsRequest request, string updatedBy)
    {
        var settings = await GetCurrentSettingsAsync();
        if (settings == null)
        {
            // Create new settings if none exist
            settings = new NotificationSettings
            {
                CreatedBy = updatedBy
            };
            _context.NotificationSettings.Add(settings);
        }
        else
        {
            settings.UpdatedBy = updatedBy;
            settings.UpdatedAt = DateTime.UtcNow;
        }

        settings.EmailNotifications = request.EmailNotifications;
        settings.SystemNotifications = request.SystemNotifications;
        settings.ReminderDays = request.ReminderDays;
        settings.EscalationDays = request.EscalationDays;
        settings.AutoReminders = request.AutoReminders;
        settings.DeadlineAlerts = request.DeadlineAlerts;
        settings.ApprovalNotifications = request.ApprovalNotifications;
        settings.RejectionNotifications = request.RejectionNotifications;
        settings.UpdateNotifications = request.UpdateNotifications;

        if (settings.Id == 0)
        {
            _context.NotificationSettings.Add(settings);
        }
        else
        {
            _context.NotificationSettings.Update(settings);
        }

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<NotificationSettings?> GetCurrentSettingsAsync()
    {
        return await _context.NotificationSettings
            .Where(n => !n.IsDeleted)
            .OrderByDescending(n => n.CreatedAt)
            .FirstOrDefaultAsync();
    }

    public async Task DeleteAsync(NotificationSettings entity)
    {
        entity.IsDeleted = true;
        _context.NotificationSettings.Update(entity);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<NotificationSettings>> FindAsync(Expression<Func<NotificationSettings, bool>> predicate)
    {
        return await _context.NotificationSettings
            .Where(predicate)
            .Where(n => !n.IsDeleted)
            .ToListAsync();
    }

    public async Task<NotificationSettings?> FirstOrDefaultAsync(Expression<Func<NotificationSettings, bool>> predicate)
    {
        return await _context.NotificationSettings
            .Where(predicate)
            .Where(n => !n.IsDeleted)
            .FirstOrDefaultAsync();
    }

    public async Task<bool> ExistsAsync(Expression<Func<NotificationSettings, bool>> predicate)
    {
        return await _context.NotificationSettings
            .Where(predicate)
            .Where(n => !n.IsDeleted)
            .AnyAsync();
    }

    public async Task<int> CountAsync(Expression<Func<NotificationSettings, bool>>? predicate = null)
    {
        var query = _context.NotificationSettings.Where(n => !n.IsDeleted);
        
        if (predicate != null)
        {
            query = query.Where(predicate);
        }
        
        return await query.CountAsync();
    }
}
