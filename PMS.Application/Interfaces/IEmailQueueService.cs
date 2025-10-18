using PMS.Application.Models.Notifications;
using PMS.Domain.Entities;

namespace PMS.Application.Interfaces;

public interface IEmailQueueService
{
    Task QueueEmailAsync(EmailMessage emailMessage, string? emailType = null, long? relatedEntityId = null, string? relatedEntityType = null);
    Task QueueOfficerWelcomeEmailAsync(User officer, string onboardingUrl);
    Task ProcessEmailQueueAsync();
}
