using MailKit.Net.Smtp;
using MimeKit;
using PMS.Application.Interfaces;
using PMS.Application.Models.Notifications;
using PMS.Domain.Entities;

namespace PMS.Application.Services;

public class EmailService : IEmailService
{
    private readonly EmailSettings _emailSettings;
    private readonly IEmailLogRepository _emailLogRepository;

    public EmailService(EmailSettings emailSettings, IEmailLogRepository emailLogRepository)
    {
        _emailSettings = emailSettings;
        _emailLogRepository = emailLogRepository;
    }

    public async Task<bool> SendEmailAsync(EmailMessage emailMessage)
    {
        return await SendEmailAsync(emailMessage, null, null, null);
    }

    public async Task<bool> SendEmailAsync(EmailMessage emailMessage, string? emailType = null, long? relatedEntityId = null, string? relatedEntityType = null)
    {
        // Validate email settings
        if (string.IsNullOrEmpty(_emailSettings.SmtpServer))
        {
            Console.WriteLine("ERROR: SMTP Server not configured");
            return false;
        }

        if (string.IsNullOrEmpty(_emailSettings.SenderEmail))
        {
            Console.WriteLine("ERROR: Sender Email not configured");
            return false;
        }

        if (string.IsNullOrEmpty(emailMessage.To))
        {
            Console.WriteLine("ERROR: Recipient email address is required");
            return false;
        }

        // Create email log entry
        var emailLog = new EmailLog
        {
            To = emailMessage.To,
            Cc = emailMessage.Cc,
            Bcc = emailMessage.Bcc,
            Subject = emailMessage.Subject,
            Body = emailMessage.Body,
            IsHtml = emailMessage.IsHtml,
            Status = "Pending",
            RetryCount = 0,
            MaxRetries = 3,
            EmailType = emailType,
            RelatedEntityId = relatedEntityId,
            RelatedEntityType = relatedEntityType,
            CreatedAt = DateTime.UtcNow
        };

        try
        {
            Console.WriteLine($"Creating email log for {emailMessage.To}");
            // Log the email attempt
            emailLog = await _emailLogRepository.CreateAsync(emailLog);
            Console.WriteLine($"Email log created with ID: {emailLog.Id}");

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_emailSettings.SenderName, _emailSettings.SenderEmail));
            message.To.Add(new MailboxAddress("", emailMessage.To));

            if (!string.IsNullOrEmpty(emailMessage.Cc))
            {
                message.Cc.Add(new MailboxAddress("", emailMessage.Cc));
            }

            if (!string.IsNullOrEmpty(emailMessage.Bcc))
            {
                message.Bcc.Add(new MailboxAddress("", emailMessage.Bcc));
            }

            message.Subject = emailMessage.Subject;

            var bodyBuilder = new BodyBuilder();
            if (emailMessage.IsHtml)
            {
                bodyBuilder.HtmlBody = emailMessage.Body;
            }
            else
            {
                bodyBuilder.TextBody = emailMessage.Body;
            }

            // Add attachments if any
            if (emailMessage.Attachments != null && emailMessage.Attachments.Any())
            {
                foreach (var attachment in emailMessage.Attachments)
                {
                    bodyBuilder.Attachments.Add(attachment.FileName, attachment.Content, ContentType.Parse(attachment.ContentType));
                }
            }

            message.Body = bodyBuilder.ToMessageBody();

            using var client = new SmtpClient();
            Console.WriteLine($"Connecting to SMTP server: {_emailSettings.SmtpServer}:{_emailSettings.SmtpPort}");
            
            // Configure SSL options based on port
            var sslOptions = _emailSettings.SmtpPort == 465 
                ? MailKit.Security.SecureSocketOptions.SslOnConnect
                : MailKit.Security.SecureSocketOptions.StartTls;
                
            Console.WriteLine($"Using SSL option: {sslOptions}");
            
            try
            {
                await client.ConnectAsync(_emailSettings.SmtpServer, _emailSettings.SmtpPort, sslOptions);
                Console.WriteLine("SMTP connection established successfully");
            }
            catch (Exception connectEx)
            {
                Console.WriteLine($"SMTP connection failed with {sslOptions}: {connectEx.Message}");
                
                // Try alternative SSL configuration for Gmail
                if (_emailSettings.SmtpServer.Contains("gmail.com"))
                {
                    Console.WriteLine("Trying alternative SSL configuration for Gmail...");
                    try
                    {
                        // Try with StartTls if SslOnConnect failed, or vice versa
                        var alternativeSslOptions = sslOptions == MailKit.Security.SecureSocketOptions.SslOnConnect
                            ? MailKit.Security.SecureSocketOptions.StartTls
                            : MailKit.Security.SecureSocketOptions.SslOnConnect;
                            
                        await client.ConnectAsync(_emailSettings.SmtpServer, _emailSettings.SmtpPort, alternativeSslOptions);
                        Console.WriteLine($"SMTP connection established with alternative SSL option: {alternativeSslOptions}");
                    }
                    catch (Exception altEx)
                    {
                        Console.WriteLine($"Alternative SSL connection also failed: {altEx.Message}");
                        throw new Exception($"Failed to connect to SMTP server. Original error: {connectEx.Message}. Alternative error: {altEx.Message}");
                    }
                }
                else
                {
                    throw;
                }
            }
            
            if (_emailSettings.RequireAuthentication)
            {
                Console.WriteLine($"Authenticating with username: {_emailSettings.Username}");
                await client.AuthenticateAsync(_emailSettings.Username, _emailSettings.Password);
            }

            Console.WriteLine($"Sending email to {emailMessage.To}");
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
            Console.WriteLine($"Email sent successfully to {emailMessage.To}");

            // Update email log as sent
            emailLog.Status = "Sent";
            emailLog.SentAt = DateTime.UtcNow;
            await _emailLogRepository.UpdateAsync(emailLog);

            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error sending email to {emailMessage.To}: {ex.Message}");
            Console.WriteLine($"Error details: {ex}");
            
            // Update email log as failed
            emailLog.Status = "Failed";
            emailLog.FailedAt = DateTime.UtcNow;
            emailLog.ErrorMessage = ex.Message;
            emailLog.RetryCount++;
            
            // Schedule retry if retries remaining
            if (emailLog.RetryCount < emailLog.MaxRetries)
            {
                emailLog.Status = "Retrying";
                emailLog.NextRetryAt = DateTime.UtcNow.AddMinutes(Math.Pow(2, emailLog.RetryCount)); // Exponential backoff
                Console.WriteLine($"Email scheduled for retry at {emailLog.NextRetryAt}");
            }
            
            await _emailLogRepository.UpdateAsync(emailLog);
            
            return false;
        }
    }

    public async Task<bool> SendEmailAsync(string to, string subject, string body, bool isHtml = true)
    {
        var emailMessage = new EmailMessage
        {
            To = to,
            Subject = subject,
            Body = body,
            IsHtml = isHtml
        };

        return await SendEmailAsync(emailMessage);
    }

    public async Task<bool> SendBulkEmailAsync(List<EmailMessage> emailMessages)
    {
        var successCount = 0;
        var tasks = emailMessages.Select(async message =>
        {
            var result = await SendEmailAsync(message);
            if (result) Interlocked.Increment(ref successCount);
            return result;
        });

        await Task.WhenAll(tasks);
        return successCount == emailMessages.Count;
    }

    public async Task<bool> RetryFailedEmailsAsync()
    {
        try
        {
            var failedEmails = await _emailLogRepository.GetFailedEmailsForRetryAsync();
            var successCount = 0;

            foreach (var emailLog in failedEmails)
            {
                var emailMessage = new EmailMessage
                {
                    To = emailLog.To,
                    Cc = emailLog.Cc,
                    Bcc = emailLog.Bcc,
                    Subject = emailLog.Subject,
                    Body = emailLog.Body,
                    IsHtml = emailLog.IsHtml
                };

                var result = await SendEmailAsync(emailMessage, emailLog.EmailType, emailLog.RelatedEntityId, emailLog.RelatedEntityType);
                if (result) successCount++;
            }

            return successCount == failedEmails.Count;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error retrying failed emails: {ex.Message}");
            return false;
        }
    }

    public async Task<List<EmailLog>> GetEmailLogsByStatusAsync(string status)
    {
        return await _emailLogRepository.GetEmailsByStatusAsync(status);
    }

    public async Task<List<EmailLog>> GetEmailLogsByTypeAsync(string emailType)
    {
        return await _emailLogRepository.GetEmailsByTypeAsync(emailType);
    }

    public async Task<List<EmailLog>> GetEmailLogsByRelatedEntityAsync(long entityId, string entityType)
    {
        return await _emailLogRepository.GetEmailsByRelatedEntityAsync(entityId, entityType);
    }
}
