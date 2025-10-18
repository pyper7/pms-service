using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PMS.Application.Interfaces;
using PMS.Application.Models;
using PMS.Application.Models.Notifications;
using PMS.Domain.Entities;

namespace PMS.API.Controllers;

[ApiController]
[Route("api/v1/emails")]
[Authorize]
public class EmailController : ControllerBase
{
    private readonly IEmailService _emailService;
    private readonly IEmailQueueService _emailQueueService;

    public EmailController(IEmailService emailService, IEmailQueueService emailQueueService)
    {
        _emailService = emailService;
        _emailQueueService = emailQueueService;
    }

    /// <summary>
    /// Get email logs by status
    /// </summary>
    [HttpGet("logs/status/{status}")]
    public async Task<ActionResult<ApiResponse<List<EmailLog>>>> GetEmailLogsByStatus(string status)
    {
        var logs = await _emailService.GetEmailLogsByStatusAsync(status);
        return Ok(ApiResponse<List<EmailLog>>.Ok(logs, "Email logs retrieved successfully"));
    }

    /// <summary>
    /// Get email logs by type
    /// </summary>
    [HttpGet("logs/type/{emailType}")]
    public async Task<ActionResult<ApiResponse<List<EmailLog>>>> GetEmailLogsByType(string emailType)
    {
        var logs = await _emailService.GetEmailLogsByTypeAsync(emailType);
        return Ok(ApiResponse<List<EmailLog>>.Ok(logs, "Email logs retrieved successfully"));
    }

    /// <summary>
    /// Get email logs by related entity
    /// </summary>
    [HttpGet("logs/entity/{entityId:long}/{entityType}")]
    public async Task<ActionResult<ApiResponse<List<EmailLog>>>> GetEmailLogsByEntity(long entityId, string entityType)
    {
        var logs = await _emailService.GetEmailLogsByRelatedEntityAsync(entityId, entityType);
        return Ok(ApiResponse<List<EmailLog>>.Ok(logs, "Email logs retrieved successfully"));
    }

    /// <summary>
    /// Retry failed emails
    /// </summary>
    [HttpPost("retry")]
    public async Task<ActionResult<ApiResponse<object>>> RetryFailedEmails()
    {
        var result = await _emailService.RetryFailedEmailsAsync();
        if (result)
        {
            return Ok(ApiResponse<object>.Ok(null, "Failed emails retry process completed successfully"));
        }
        else
        {
            return BadRequest(ApiResponse<object>.Fail("Some emails failed to retry", "RETRY_FAILED"));
        }
    }

    /// <summary>
    /// Get all email logs (for debugging)
    /// </summary>
    [HttpGet("logs")]
    public async Task<ActionResult<ApiResponse<List<EmailLog>>>> GetAllEmailLogs()
    {
        var pendingLogs = await _emailService.GetEmailLogsByStatusAsync("Pending");
        var sentLogs = await _emailService.GetEmailLogsByStatusAsync("Sent");
        var failedLogs = await _emailService.GetEmailLogsByStatusAsync("Failed");
        var retryingLogs = await _emailService.GetEmailLogsByStatusAsync("Retrying");

        var allLogs = pendingLogs.Concat(sentLogs).Concat(failedLogs).Concat(retryingLogs)
            .OrderByDescending(x => x.CreatedAt)
            .ToList();

        return Ok(ApiResponse<List<EmailLog>>.Ok(allLogs, "All email logs retrieved successfully"));
    }

    /// <summary>
    /// Validate email configuration
    /// </summary>
    [HttpGet("config/validate")]
    public ActionResult<ApiResponse<object>> ValidateEmailConfig()
    {
        try
        {
            // This will be implemented to check if email settings are properly configured
            return Ok(ApiResponse<object>.Ok(new { 
                message = "Email configuration validation endpoint ready",
                timestamp = DateTime.UtcNow
            }, "Email configuration validation completed"));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<object>.Fail($"Error validating email config: {ex.Message}", "CONFIG_ERROR"));
        }
    }

    /// <summary>
    /// Send test email (for debugging email configuration)
    /// </summary>
    [HttpPost("test")]
    public async Task<ActionResult<ApiResponse<object>>> SendTestEmail([FromBody] TestEmailRequest request)
    {
        try
        {
            var emailMessage = new EmailMessage
            {
                To = request.To,
                Subject = "TETFund PMS - Test Email",
                Body = $@"
                    <html>
                    <body>
                        <h2>Test Email from TETFund PMS</h2>
                        <p>This is a test email to verify email configuration.</p>
                        <p><strong>Time:</strong> {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC</p>
                        <p><strong>From:</strong> TETFund Performance Management System</p>
                        <hr>
                        <p><em>If you received this email, your email configuration is working correctly!</em></p>
                    </body>
                    </html>",
                IsHtml = true
            };

            // Queue the test email for background processing
            await _emailQueueService.QueueEmailAsync(emailMessage, "TestEmail", null, "Test");
            
            return Ok(ApiResponse<object>.Ok(null, "Test email queued for sending"));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<object>.Fail($"Error queuing test email: {ex.Message}", "EMAIL_ERROR"));
        }
    }
}
