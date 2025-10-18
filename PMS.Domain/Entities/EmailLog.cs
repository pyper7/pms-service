using System.ComponentModel.DataAnnotations;

namespace PMS.Domain.Entities;

public class EmailLog : BaseEntity
{
    [Required]
    [MaxLength(500)]
    public string To { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Cc { get; set; }

    [MaxLength(500)]
    public string? Bcc { get; set; }

    [Required]
    [MaxLength(200)]
    public string Subject { get; set; } = string.Empty;

    [Required]
    public string Body { get; set; } = string.Empty;

    public bool IsHtml { get; set; } = true;

    [Required]
    [MaxLength(50)]
    public string Status { get; set; } = "Pending"; // Pending, Sent, Failed, Retrying

    public int RetryCount { get; set; } = 0;

    public int MaxRetries { get; set; } = 3;

    public DateTime? SentAt { get; set; }

    public DateTime? FailedAt { get; set; }

    [MaxLength(1000)]
    public string? ErrorMessage { get; set; }

    [MaxLength(100)]
    public string? EmailType { get; set; } // OfficerWelcome, PasswordReset, etc.

    public long? RelatedEntityId { get; set; } // ID of the related entity (e.g., Officer ID)

    [MaxLength(100)]
    public string? RelatedEntityType { get; set; } // Type of related entity (e.g., "Officer")

    public DateTime? NextRetryAt { get; set; }
}
