namespace Module.Inventory.Features.Storefront.ReserveCartStock;

public sealed record ReserveCartStockCommand : ICommand<ReserveCartStockResponse>
{
    public Guid CartId { get; init; }
    public IReadOnlyList<ReserveLineItem> LineItems { get; init; } = [];
    public int TtlMinutes { get; init; } = 30;
}

public sealed record ReserveLineItem
{
    public Guid VariantId { get; init; }
    public int Quantity { get; init; }
}
