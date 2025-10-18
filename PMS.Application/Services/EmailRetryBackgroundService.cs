using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PMS.Application.Interfaces;

namespace PMS.Application.Services;

public class EmailRetryBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<EmailRetryBackgroundService> _logger;
    private readonly TimeSpan _retryInterval = TimeSpan.FromMinutes(5); // Retry every 5 minutes

    public EmailRetryBackgroundService(IServiceProvider serviceProvider, ILogger<EmailRetryBackgroundService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

                var retryResult = await emailService.RetryFailedEmailsAsync();
                
                if (retryResult)
                {
                    _logger.LogInformation("Email retry process completed successfully");
                }
                else
                {
                    _logger.LogWarning("Some emails failed to retry");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during email retry process");
            }

            await Task.Delay(_retryInterval, stoppingToken);
        }
    }
}
