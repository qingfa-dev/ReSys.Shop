namespace Module.Ordering.Features.Storefront.Cart.Shared.Models;

public abstract record CartParameters
{
    public Guid VariantId { get; init; }
    public int Quantity { get; init; } = 1;
    public string? Notes { get; init; }
}