using Module.Inventory.Services.StockReservations;

namespace Module.Inventory.Backgrounds;

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
        var result = await _reservationService.ExpireReservationsAsync(ct);
        var expiredCount = result.IsSuccess ? result.Value : 0;

        if (expiredCount > 0)
            ReservationExpiryJobLoggers.SweepCompleted(_logger, expiredCount);

        return expiredCount;
    }
}