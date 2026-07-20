using Module.Inventory.Services.Abstractions;

namespace Module.Inventory.Backgrounds;

/// <summary>Background job that expires overdue stock reservations and restores inventory.</summary>
public sealed class ReservationExpiryJob
{
    private readonly IStockReservationService _reservationService;
    private readonly ILogger<ReservationExpiryJob> _logger;

    public ReservationExpiryJob(
        IStockReservationService reservationService,
        ILogger<ReservationExpiryJob> logger)
    {
        _reservationService = reservationService;
        _logger = logger;
    }

    /// <summary>Executes the reservation expiry sweep and returns count of expired reservations.</summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The number of expired reservations processed.</returns>
    public async Task<int> RunAsync(CancellationToken ct = default)
    {
        var expiredCount = await _reservationService.ExpireReservationsAndRestoreStockAsync(ct);

        if (expiredCount > 0)
            ReservationExpiryJobLoggers.SweepCompleted(_logger, expiredCount);

        return expiredCount;
    }
}