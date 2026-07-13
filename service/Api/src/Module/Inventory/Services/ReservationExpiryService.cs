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
        // Log: Reservation expiry sweep started
        ReservationExpiryLoggers.ServiceStarted(_logger);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                // Await: Wait for the sweep interval before next check
                await Task.Delay(SweepInterval, stoppingToken);

                using var scope = _scopeFactory.CreateScope();
                // Call: Acquire reservation service via DI scope
                var reservationService = scope.ServiceProvider.GetRequiredService<IStockReservationService>();

                var expiredCount = await reservationService.ExpireReservationsAndRestoreStockAsync(stoppingToken);

                if (expiredCount > 0)
                    // Log: Report count of expired and restored reservations
                    ReservationExpiryLoggers.SweepCompleted(_logger, expiredCount);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                // Log: Non-fatal error during sweep — will retry on next interval
                ReservationExpiryLoggers.SweepError(_logger, ex);
            }
        }

        // Log: Reservation expiry sweep stopped
        ReservationExpiryLoggers.ServiceStopped(_logger);
    }
}

public static partial class ReservationExpiryLoggers
{
    [LoggerMessage(EventId = 2000, Level = LogLevel.Information, Message = "Expired {Count} stock reservations and restored stock")]
    public static partial void SweepCompleted(ILogger logger, int Count);

    [LoggerMessage(EventId = 2001, Level = LogLevel.Information, Message = "Reservation expiry service started")]
    public static partial void ServiceStarted(ILogger logger);

    [LoggerMessage(EventId = 2002, Level = LogLevel.Error, Message = "Error during reservation expiry sweep")]
    public static partial void SweepError(ILogger logger, Exception ex);

    [LoggerMessage(EventId = 2003, Level = LogLevel.Information, Message = "Reservation expiry service stopped")]
    public static partial void ServiceStopped(ILogger logger);
}