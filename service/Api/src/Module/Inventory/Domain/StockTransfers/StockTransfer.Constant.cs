namespace Module.Inventory.Domain.StockTransfers;

public static class StockTransferConstant
{
    public static class Constraints
    {
        public const int NumberMaxLength = 50;
        public const int ReferenceMaxLength = 255;
        public const int MaxStateLength = 20;
    }

    public static class Defaults
    {
    }

    public static class Query
    {
        public static readonly string[] AllowedSearchFields = [];

        public static readonly string[] AllowedSortFields =
        [
            nameof(StockTransfer.Number),
            nameof(StockTransfer.State),
            nameof(StockTransfer.CreatedAtUtc)
        ];

        public static readonly string[] AllowedFilterFields =
        [
            nameof(StockTransfer.State),
            nameof(StockTransfer.SourceLocationId),
            nameof(StockTransfer.DestinationLocationId)
        ];
    }
}