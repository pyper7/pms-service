using PMS.Domain.Entities;

namespace PMS.Application.Interfaces;

public interface IEmailLogRepository
{
    Task<EmailLog> CreateAsync(EmailLog emailLog);
    Task<EmailLog?> GetByIdAsync(long id);
    Task<List<EmailLog>> GetPendingEmailsAsync();
    Task<List<EmailLog>> GetFailedEmailsForRetryAsync();
    Task UpdateAsync(EmailLog emailLog);
    Task<List<EmailLog>> GetEmailsByStatusAsync(string status);
    Task<List<EmailLog>> GetEmailsByTypeAsync(string emailType);
    Task<List<EmailLog>> GetEmailsByRelatedEntityAsync(long entityId, string entityType);
}
