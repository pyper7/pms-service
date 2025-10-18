using System.ComponentModel.DataAnnotations;

namespace PMS.Domain.Entities;

public class NotificationSettings : BaseEntity
{
    public bool EmailNotifications { get; set; } = true;
    public bool SystemNotifications { get; set; } = true;
    public int ReminderDays { get; set; } = 7;
    public int EscalationDays { get; set; } = 3;
    public bool AutoReminders { get; set; } = true;
    public bool DeadlineAlerts { get; set; } = true;
    public bool ApprovalNotifications { get; set; } = true;
    public bool RejectionNotifications { get; set; } = true;
    public bool UpdateNotifications { get; set; } = true;
}
