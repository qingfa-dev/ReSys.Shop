namespace Module.Inventory.Domain.StockLocations.StockItems;

public static class StockItemConstant
{
    public static class Constraints
    {
        // StockItem has no string properties requiring length constraints
    }

    public static class Defaults
    {
        public const int CountOnHand = 0;
        public const bool Backorderable = false;
    }

    public static class Query
    {
        public static readonly string[] AllowedSearchFields = [];

        public static readonly string[] AllowedSortFields =
        [
            nameof(StockItem.CountOnHand),
            nameof(StockItem.CreatedAtUtc)
        ];

        public static readonly string[] AllowedFilterFields =
        [
            nameof(StockItem.StockLocationId),
            nameof(StockItem.VariantId),
            nameof(StockItem.Backorderable)
        ];
    }
}