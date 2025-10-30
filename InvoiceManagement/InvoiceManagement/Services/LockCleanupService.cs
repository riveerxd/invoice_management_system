namespace InvoiceManagement.Services;

public class LockCleanupService : IHostedService, IDisposable
{
    private readonly ILogger<LockCleanupService> _logger;
    private readonly IServiceProvider _serviceProvider;
    private Timer? _timer;

    public LockCleanupService(ILogger<LockCleanupService> logger, IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Lock Cleanup Service starting");

        // Run cleanup every 1 minute
        _timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromMinutes(1));

        return Task.CompletedTask;
    }

    private async void DoWork(object? state)
    {
        _logger.LogDebug("Lock Cleanup Service is working");

        try
        {
            using var scope = _serviceProvider.CreateScope();
            var lockService = scope.ServiceProvider.GetRequiredService<ILockService>();

            await lockService.CleanupExpiredLocksAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred executing Lock Cleanup Service");
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Lock Cleanup Service stopping");

        _timer?.Change(Timeout.Infinite, 0);

        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _timer?.Dispose();
    }
}
