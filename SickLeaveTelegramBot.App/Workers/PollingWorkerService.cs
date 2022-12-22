using SickLeaveTelegramBot.App.Contracts;

namespace SickLeaveTelegramBot.App.Workers;

public class PollingWorkerService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<PollingWorkerService> _logger;

    public PollingWorkerService(
        IServiceProvider serviceProvider,
        ILogger<PollingWorkerService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Starting polling service");

        await DoWork(stoppingToken);
    }

    private async Task DoWork(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var receiver = scope.ServiceProvider.GetRequiredService<IReceiverService>();

                await receiver.ReceiveAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError("Polling failed with exception: {Exception}", ex);

                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }
    }
}