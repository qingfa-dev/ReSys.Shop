namespace Module.Inventory.Services;

internal sealed partial class StockReservationService
{
    internal static partial class Loggers
    {
        [LoggerMessage(EventId = 3200, Level = LogLevel.Information, Message = "Reservation created: variant={VariantId}, quantity={Quantity}, ttl={TtlMinutes}min")]
        public static partial void LogReservationCreated(ILogger logger, Guid variantId, int quantity, int ttlMinutes);

        [LoggerMessage(EventId = 3201, Level = LogLevel.Warning, Message = "Reservation failed: variant={VariantId}, reason={Reason}")]
        public static partial void LogReservationFailed(ILogger logger, Guid variantId, string reason);

        [LoggerMessage(EventId = 3202, Level = LogLevel.Information, Message = "Released {Count} reservations")]
        public static partial void LogReservationsReleased(ILogger logger, int count);

        [LoggerMessage(EventId = 3203, Level = LogLevel.Information, Message = "Expired {Count} reservations")]
        public static partial void LogReservationsExpired(ILogger logger, int count);

        [LoggerMessage(EventId = 3204, Level = LogLevel.Warning, Message = "Failed to fulfill reservation {ReservationId}: {Reason}")]
        public static partial void LogFulfillmentFailed(ILogger logger, Guid reservationId, string reason);
    }
}
