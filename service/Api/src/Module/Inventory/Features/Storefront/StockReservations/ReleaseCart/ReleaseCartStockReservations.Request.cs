namespace Module.Inventory.Features.Storefront.StockReservations.ReleaseCart;

public static partial class ReleaseCartStockReservations
{
    public sealed record Request
    {
        public Guid CartId { get; init; }
    }
}
