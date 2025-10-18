using PMS.Application.Models.AppraisalSettings;
using PMS.Domain.Entities;

namespace PMS.Application.Interfaces;

public interface INotificationSettingsRepository : IRepository<NotificationSettings>
{
    Task<NotificationSettingsDto> GetSettingsAsync();
    Task<bool> UpdateSettingsAsync(UpdateNotificationSettingsRequest request, string updatedBy);
    Task<NotificationSettings?> GetCurrentSettingsAsync();
}
