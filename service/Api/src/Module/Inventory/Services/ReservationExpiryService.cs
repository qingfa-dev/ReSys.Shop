using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Module.Inventory.Services.Abstractions;

namespace Module.Inventory.Services;

/// <summary>
/// Background service that periodically sweeps expired stock reservations,
/// restoring stock and marking reservations as Expired.
/// </summary>
public class ReservationExpiryService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<ReservationExpiryService> _logger;
    internal static TimeSpan SweepInterval = TimeSpan.FromSeconds(60);

    public ReservationExpiryService(
        IServiceScopeFactory scopeFactory,
        ILogger<ReservationExpiryService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Reservation expiry service started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(SweepInterval, stoppingToken);

                using var scope = _scopeFactory.CreateScope();
                var stockChecker = scope.ServiceProvider.GetRequiredService<IStockChecker>();

                var expiredCount = await stockChecker.ExpireReservationsAndRestoreStockAsync(stoppingToken);

                if (expiredCount > 0)
                    ReservationExpiryLoggers.SweepCompleted(_logger, expiredCount);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during reservation expiry sweep");
            }
        }

        _logger.LogInformation("Reservation expiry service stopped");
    }
}

public static partial class ReservationExpiryLoggers
{
    [Microsoft.Extensions.Logging.LoggerMessage(EventId = 2000, Level = Microsoft.Extensions.Logging.LogLevel.Information, Message = "Expired {Count} stock reservations and restored stock")]
    public static partial void SweepCompleted(Microsoft.Extensions.Logging.ILogger logger, int Count);
}
