using Module.Inventory.Services.Abstractions;

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

    public async Task<int> RunAsync(CancellationToken ct = default)
    {
        var expiredCount = await _reservationService.ExpireReservationsAndRestoreStockAsync(ct);

        if (expiredCount > 0)
            ReservationExpiryJobLoggers.SweepCompleted(_logger, expiredCount);

        return expiredCount;
    }
}