namespace PMS.Application.Models.AppraisalSettings;

public class NotificationSettingsDto
{
    public bool EmailNotifications { get; set; }
    public bool SystemNotifications { get; set; }
    public int ReminderDays { get; set; }
    public int EscalationDays { get; set; }
    public bool AutoReminders { get; set; }
    public bool DeadlineAlerts { get; set; }
    public bool ApprovalNotifications { get; set; }
    public bool RejectionNotifications { get; set; }
    public bool UpdateNotifications { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class UpdateNotificationSettingsRequest
{
    public bool EmailNotifications { get; set; }
    public bool SystemNotifications { get; set; }
    public int ReminderDays { get; set; }
    public int EscalationDays { get; set; }
    public bool AutoReminders { get; set; }
    public bool DeadlineAlerts { get; set; }
    public bool ApprovalNotifications { get; set; }
    public bool RejectionNotifications { get; set; }
    public bool UpdateNotifications { get; set; }
}
