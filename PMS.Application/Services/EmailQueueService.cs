using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PMS.Application.Interfaces;
using PMS.Application.Models.Notifications;
using PMS.Domain.Entities;

namespace PMS.Application.Services;

public class EmailQueueService : IEmailQueueService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailQueueService> _logger;

    public EmailQueueService(
        IServiceProvider serviceProvider,
        IConfiguration configuration,
        ILogger<EmailQueueService> logger)
    {
        _serviceProvider = serviceProvider;
        _configuration = configuration;
        _logger = logger;
    }

    public Task QueueEmailAsync(EmailMessage emailMessage, string? emailType = null, long? relatedEntityId = null, string? relatedEntityType = null)
    {
        // Create a new scope for the background task
        _ = Task.Run(async () =>
        {
            using var scope = _serviceProvider.CreateScope();
            try
            {
                var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();
                _logger.LogInformation("Processing email for {Recipient}", emailMessage.To);
                await emailService.SendEmailAsync(emailMessage, emailType, relatedEntityId, relatedEntityType);
                _logger.LogInformation("Successfully processed email for {Recipient}", emailMessage.To);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process email for {Recipient}", emailMessage.To);
            }
        });
        
        return Task.CompletedTask;
    }

    public async Task QueueOfficerWelcomeEmailAsync(User officer, string onboardingUrl)
    {
        try
        {
            _logger.LogInformation("Queueing welcome email for officer {Email}", officer.Email);
            
            // Create email message in the current scope
            using var scope = _serviceProvider.CreateScope();
            var emailTemplateService = scope.ServiceProvider.GetRequiredService<IEmailTemplateService>();
            var emailMessage = await emailTemplateService.CreateOfficerWelcomeEmailAsync(officer, onboardingUrl);
            
            // Queue the email for background processing (fire and forget)
            QueueEmailAsync(emailMessage, "OfficerWelcome", officer.Id, "Officer");
            
            _logger.LogInformation("Welcome email queued for officer {Email}", officer.Email);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to queue welcome email for officer {Email}", officer.Email);
        }
    }

    public async Task ProcessEmailQueueAsync()
    {
        // This method can be used for processing any pending emails
        // For now, we're using immediate processing with Task.Run
        await Task.CompletedTask;
    }
}
