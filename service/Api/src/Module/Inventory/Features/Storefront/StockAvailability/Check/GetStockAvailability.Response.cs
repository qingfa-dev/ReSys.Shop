namespace Module.Inventory.Features.Storefront.StockAvailability.Check;

public static partial class GetStockAvailability
{
    public class Response
    {
        public Guid VariantId { get; init; }
        public int TotalOnHand { get; init; }
        public int TotalReserved { get; init; }
        public int CartReserved { get; init; }
        public int TotalAvailable { get; init; }
        public int AvailableToCart { get; init; }
        public List<LocationAvailability> LocationAvailability { get; init; } = [];
    }

    public sealed record LocationAvailability
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