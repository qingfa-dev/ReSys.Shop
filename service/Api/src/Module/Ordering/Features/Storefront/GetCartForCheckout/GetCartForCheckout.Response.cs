using Module.Ordering.Domain.Orders;

namespace Module.Ordering.Features.Storefront.GetCartForCheckout;

public sealed record GetCartForCheckoutResponse
{
    public CheckoutState State { get; init; }
    public IReadOnlyList<CartLineItem> LineItems { get; init; } = [];
    public decimal Total { get; init; }
    public string? Email { get; init; }
}

public sealed record CartLineItem
{
    public Guid VariantId { get; init; }
    public int Quantity { get; init; }
}
