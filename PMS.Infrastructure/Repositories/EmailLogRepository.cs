using Microsoft.EntityFrameworkCore;
using PMS.Application.Interfaces;
using PMS.Domain.Entities;
using PMS.Infrastructure.Data;

namespace PMS.Infrastructure.Repositories;

public class EmailLogRepository : IEmailLogRepository
{
    private readonly PmsDbContext _context;

    public EmailLogRepository(PmsDbContext context)
    {
        _context = context;
    }

    public async Task<EmailLog> CreateAsync(EmailLog emailLog)
    {
        _context.EmailLogs.Add(emailLog);
        await _context.SaveChangesAsync();
        return emailLog;
    }

    public async Task<EmailLog?> GetByIdAsync(long id)
    {
        return await _context.EmailLogs.FindAsync(id);
    }

    public async Task<List<EmailLog>> GetPendingEmailsAsync()
    {
        return await _context.EmailLogs
            .Where(e => e.Status == "Pending")
            .OrderBy(e => e.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<EmailLog>> GetFailedEmailsForRetryAsync()
    {
        var now = DateTime.UtcNow;
        return await _context.EmailLogs
            .Where(e => e.Status == "Failed" && 
                       e.RetryCount < e.MaxRetries && 
                       (e.NextRetryAt == null || e.NextRetryAt <= now))
            .OrderBy(e => e.NextRetryAt)
            .ToListAsync();
    }

    public async Task UpdateAsync(EmailLog emailLog)
    {
        _context.EmailLogs.Update(emailLog);
        await _context.SaveChangesAsync();
    }

    public async Task<List<EmailLog>> GetEmailsByStatusAsync(string status)
    {
        return await _context.EmailLogs
            .Where(e => e.Status == status)
            .OrderByDescending(e => e.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<EmailLog>> GetEmailsByTypeAsync(string emailType)
    {
        return await _context.EmailLogs
            .Where(e => e.EmailType == emailType)
            .OrderByDescending(e => e.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<EmailLog>> GetEmailsByRelatedEntityAsync(long entityId, string entityType)
    {
        return await _context.EmailLogs
            .Where(e => e.RelatedEntityId == entityId && e.RelatedEntityType == entityType)
            .OrderByDescending(e => e.CreatedAt)
            .ToListAsync();
    }
}
