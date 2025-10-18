using PMS.Application.Models;
using PMS.Application.Models.AppraisalSettings;

namespace PMS.Application.Interfaces;

public interface INotificationSettingsService
{
    Task<ApiResponse<NotificationSettingsDto>> GetNotificationSettingsAsync();
    Task<ApiResponse<NotificationSettingsDto>> UpdateNotificationSettingsAsync(UpdateNotificationSettingsRequest request, string updatedBy);
}
