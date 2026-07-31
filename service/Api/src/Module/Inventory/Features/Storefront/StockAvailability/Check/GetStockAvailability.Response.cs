namespace Module.Inventory.Features.Storefront.StockAvailability.Check;

public static partial class GetStockAvailability
{
    // EXCEPTION: computed availability DTO — composite of stock item + reservation data
    public sealed record Response
    {
        public Guid StockLocationId { get; init; }
        public string LocationName { get; init; } = string.Empty;
        public int CountOnHand { get; init; }
        public int ReservedCount { get; init; }
        public int AvailableCount { get; init; }
        public bool Backorderable { get; init; }
        public bool Available { get; init; }
    }
}
