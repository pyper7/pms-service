using PMS.Application.Models.Notifications;
using PMS.Domain.Entities;

namespace PMS.Application.Interfaces;

public interface IEmailTemplateService
{
    Task<EmailMessage> CreateOfficerWelcomeEmailAsync(User officer, string onboardingUrl);
    string ProcessEmailTemplate(string template, Dictionary<string, string> placeholders);
}
