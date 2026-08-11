namespace Module.Inventory.Features.Storefront.StockReservations.Reserve;

public static partial class ReserveStockReservation
{
    public sealed record Request
    {
        public Guid VariantId { get; init; }
        public Guid StockLocationId { get; init; }
        public int Quantity { get; init; }
        public int TtlMinutes { get; init; } = 15;
    }
}
