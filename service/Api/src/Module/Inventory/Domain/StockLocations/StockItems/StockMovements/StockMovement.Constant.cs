namespace Module.Inventory.Domain.StockLocations.StockItems.StockMovements;

public static class StockMovementConstant
{
    public static class Constraints
    {
        public const int MaxReasonLength = 500;
    }

    public static class Query
    {
        public static readonly string[] AllowedSearchFields =
        [
            nameof(StockMovement.Reason)
        ];

        public static readonly string[] AllowedSortFields =
        [
            nameof(StockMovement.Quantity),
            nameof(StockMovement.CreatedAtUtc)
        ];

        public static readonly string[] AllowedFilterFields =
        [
            nameof(StockMovement.StockItemId),
            nameof(StockMovement.OriginatorType)
        ];
    }
}
