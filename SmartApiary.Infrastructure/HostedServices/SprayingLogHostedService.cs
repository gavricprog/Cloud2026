using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SmartApiary.Application.Interfaces;

namespace SmartApiary.Infrastructure.HostedServices;

public class SprayingLogHostedService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<SprayingLogHostedService> _logger;

    public SprayingLogHostedService(IServiceScopeFactory scopeFactory, ILogger<SprayingLogHostedService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var completion = scope.ServiceProvider.GetRequiredService<ISprayingCompletionService>();
                var count = await completion.CompleteExpiredAnnouncementsAsync(stoppingToken);
                if (count > 0)
                    _logger.LogInformation("Evidentirano {Count} završenih tretmana prskanja.", count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška pri automatskom završetku tretmana prskanja.");
            }

            await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
        }
    }
}
