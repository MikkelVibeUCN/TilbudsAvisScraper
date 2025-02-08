using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly SmartScheduler _scheduler;

    public Worker(ILogger<Worker> logger, SmartScheduler scheduler)
    {
        _logger = logger;
        _scheduler = scheduler;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Worker started at: {time}", DateTimeOffset.Now);
        await _scheduler.InitializeAsync();

        // Keep the service running until stopped
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Worker stopping...");
        await base.StopAsync(cancellationToken);
    }
}