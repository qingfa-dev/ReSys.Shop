using Module.Ordering.Domain.Orders;

namespace Module.Ordering.Features.Storefront.Cart.Shared.Models;

public record CartItem
{
    public Guid Id { get; init; }
    public Guid VariantId { get; init; }
    public string VariantName { get; init; } = string.Empty;
    public string Sku { get; init; } = string.Empty;
    public int Quantity { get; init; }
    public decimal Price { get; init; }
    public decimal Total { get; init; }
}

public record CartDetailResponse : CartResponseBase
{
    public List<CartItem> Items { get; init; } = [];
}

public record CartListItemResponse : CartResponseBase;
