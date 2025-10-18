using PMS.Application.Interfaces;
using PMS.Application.Models;
using PMS.Application.Models.AppraisalSettings;

namespace PMS.Application.Services;

public class NotificationSettingsService : INotificationSettingsService
{
    private readonly INotificationSettingsRepository _notificationSettingsRepository;

    public NotificationSettingsService(INotificationSettingsRepository notificationSettingsRepository)
    {
        _notificationSettingsRepository = notificationSettingsRepository;
    }

    public async Task<ApiResponse<NotificationSettingsDto>> GetNotificationSettingsAsync()
    {
        try
        {
            var result = await _notificationSettingsRepository.GetSettingsAsync();
            return ApiResponse<NotificationSettingsDto>.Ok(result);
        }
        catch (Exception ex)
        {
            return ApiResponse<NotificationSettingsDto>.Fail($"Error retrieving notification settings: {ex.Message}", "INTERNAL_ERROR");
        }
    }

    public async Task<ApiResponse<NotificationSettingsDto>> UpdateNotificationSettingsAsync(UpdateNotificationSettingsRequest request, string updatedBy)
    {
        try
        {
            // Validate request
            if (request.ReminderDays < 1 || request.ReminderDays > 30)
            {
                return ApiResponse<NotificationSettingsDto>.Fail("Reminder days must be between 1 and 30", "VALIDATION_ERROR");
            }

            if (request.EscalationDays < 1 || request.EscalationDays > 30)
            {
                return ApiResponse<NotificationSettingsDto>.Fail("Escalation days must be between 1 and 30", "VALIDATION_ERROR");
            }

            if (request.EscalationDays >= request.ReminderDays)
            {
                return ApiResponse<NotificationSettingsDto>.Fail("Escalation days must be less than reminder days", "VALIDATION_ERROR");
            }

            var success = await _notificationSettingsRepository.UpdateSettingsAsync(request, updatedBy);
            if (!success)
            {
                return ApiResponse<NotificationSettingsDto>.Fail("Failed to update notification settings", "INTERNAL_ERROR");
            }

            var result = await _notificationSettingsRepository.GetSettingsAsync();
            return ApiResponse<NotificationSettingsDto>.Ok(result, "Notification settings updated successfully");
        }
        catch (Exception ex)
        {
            return ApiResponse<NotificationSettingsDto>.Fail($"Error updating notification settings: {ex.Message}", "INTERNAL_ERROR");
        }
    }
}
