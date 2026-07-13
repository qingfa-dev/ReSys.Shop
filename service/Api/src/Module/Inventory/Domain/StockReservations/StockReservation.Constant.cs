namespace Module.Inventory.Domain.StockReservations;

public static class StockReservationConstant
{
    public static class Defaults
    {
        public const int DefaultTtlMinutes = 15;
    }

    public static class Constraints
    {
        public const int MaxReasonLength = 255;
    }

    public static class Query
    {
        public static readonly string[] AllowedFilterFields =
        [
            nameof(StockReservation.VariantId),
            nameof(StockReservation.OrderId),
            nameof(StockReservation.State)
        ];

        public static readonly string[] AllowedSortFields =
        [
            nameof(StockReservation.ExpiresAtUtc),
            nameof(StockReservation.CreatedAtUtc)
        ];
    }
}