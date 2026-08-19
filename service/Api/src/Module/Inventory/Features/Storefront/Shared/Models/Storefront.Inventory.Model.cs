namespace Module.Inventory.Features.Storefront.Shared.Models;

public sealed record ReserveLineItem
{
    public Guid VariantId { get; init; }
    public int Quantity { get; init; }
}
