namespace Module.Inventory.Features.Storefront.StockReservations.ConsumeCart;

public static partial class ConsumeCartStockReservations
{
    public sealed record Request
    {
        public Guid CartId { get; init; }
    }
}
