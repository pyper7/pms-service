using PMS.Application.Models.Notifications;
using PMS.Domain.Entities;

namespace PMS.Application.Interfaces;

public interface IEmailService
{
    Task<bool> SendEmailAsync(EmailMessage emailMessage);
    Task<bool> SendEmailAsync(EmailMessage emailMessage, string? emailType = null, long? relatedEntityId = null, string? relatedEntityType = null);
    Task<bool> SendEmailAsync(string to, string subject, string body, bool isHtml = true);
    Task<bool> SendBulkEmailAsync(List<EmailMessage> emailMessages);
    Task<bool> RetryFailedEmailsAsync();
    Task<List<EmailLog>> GetEmailLogsByStatusAsync(string status);
    Task<List<EmailLog>> GetEmailLogsByTypeAsync(string emailType);
    Task<List<EmailLog>> GetEmailLogsByRelatedEntityAsync(long entityId, string entityType);
}
