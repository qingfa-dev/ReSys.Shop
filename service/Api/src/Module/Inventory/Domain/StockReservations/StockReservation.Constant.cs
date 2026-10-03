namespace Module.Inventory.Domain.StockReservations;

public static class StockReservationConstant
{
    public static class Defaults
    {
        public const int DefaultTtlMinutes = 15;
        public const int MinTtlMinutes = 1;
        public const int MaxTtlMinutes = 10080;     // 7 days
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

        public static readonly string[] AllowedSearchFields = [];
    }
}