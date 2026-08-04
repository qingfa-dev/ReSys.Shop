using Module.Ordering.Domain.Orders;

namespace Module.Ordering.Features.Storefront.Cart.Shared.Models;

/// <summary>Line item within a cart response — identifies the variant, quantity, and computed totals.</summary>
public record CartItem : Response
{
    /// <summary>The product variant added to the cart.</summary>
    public Guid VariantId { get; init; }
    /// <summary>Display name of the product variant.</summary>
    public string VariantName { get; init; } = string.Empty;
    /// <summary>Stock-keeping unit code for the variant.</summary>
    public string Sku { get; init; } = string.Empty;
    /// <summary>Display name of the product (from the product entity).</summary>
    public string? ProductName { get; init; }
    /// <summary>Primary image URL of the product.</summary>
    public string? ProductImageUrl { get; init; }
    /// <summary>Number of units in this line item.</summary>
    public int Quantity { get; init; }
    /// <summary>Unit price at the time of addition.</summary>
    public decimal Price { get; init; }
    /// <summary>Computed total for this line item (Price × Quantity).</summary>
    public decimal Total { get; init; }
}

/// <summary>Cart detail response — includes line items and computed cart totals.</summary>
public record CartDetailResponse : CartResponseBase, IResponse
{
    /// <summary>Line items currently in the cart.</summary>
    public List<CartItem> Items { get; init; } = [];
}

/// <summary>Cart list item — lightweight summary for cart list views without line items.</summary>
public record CartListItemResponse : CartResponseBase, IResponse;