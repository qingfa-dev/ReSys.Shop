namespace Module.Inventory.Features.Storefront.ReserveCartStock;

public sealed record ReserveCartStockResponse
{
    public IReadOnlyList<Guid> ReservationIds { get; init; } = [];
}
