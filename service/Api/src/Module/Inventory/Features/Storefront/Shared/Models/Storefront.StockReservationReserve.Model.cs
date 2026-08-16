namespace Module.Inventory.Features.Storefront.Shared.Models;

public abstract record StockReservationReserveParameters
{
    public Guid VariantId { get; init; }
    public Guid StockLocationId { get; init; }
    public int Quantity { get; init; }
    public int TtlMinutes { get; init; } = 15;
}
