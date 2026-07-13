using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Module.Ordering.Backgrounds;

namespace Module.Ordering.Services;

public sealed partial class CartExpiryService : BackgroundService
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

    /// <summary>Periodically sweeps expired draft carts by running a background job on a configurable interval.</summary>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Log: Service started — sweep interval is {DefaultSweepInterval.TotalHours} hours.
        Loggers.Started(_logger);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                // Await: Delay for the configured sweep interval before next run.
                await Task.Delay(DefaultSweepInterval, stoppingToken);

                using var scope = _scopeFactory.CreateScope();
                var job = scope.ServiceProvider.GetRequiredService<CartExpiryJob>();

                // Await: Execute the cart expiry job.
                await job.RunAsync(stoppingToken);
            }
            // Catch: Shutdown requested — exit loop cleanly.
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            // Catch: Log and continue on unexpected errors to keep the sweep loop alive.
            catch (Exception ex)
            {
                Loggers.SweepError(_logger, ex);
            }
        }
    }
}