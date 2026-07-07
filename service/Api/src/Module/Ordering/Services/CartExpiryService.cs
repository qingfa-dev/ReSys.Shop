using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using Module.Ordering.Backgrounds;

namespace Module.Ordering.Services;

public sealed class CartExpiryService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<CartExpiryService> _logger;

    internal static TimeSpan DefaultSweepInterval = TimeSpan.FromHours(1);

    public CartExpiryService(
        IServiceScopeFactory scopeFactory,
        IConfiguration configuration,
        ILogger<CartExpiryService> logger)
    {
        _scopeFactory = scopeFactory;
        _configuration = configuration;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Cart-expiry service started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(DefaultSweepInterval, stoppingToken);

                using var scope = _scopeFactory.CreateScope();
                var job = scope.ServiceProvider.GetRequiredService<CartExpiryJob>();

                await job.RunAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during cart expiry sweep");
            }
        }
    }
}
