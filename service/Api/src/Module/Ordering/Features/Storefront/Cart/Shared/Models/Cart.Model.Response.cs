namespace Module.Ordering.Features.Storefront.Cart.Shared.Models;

/// <summary>Line item within a cart response — identifies the variant, quantity, and computed totals.</summary>
public record CartItem
{
    /// <summary>The product variant added to the cart.</summary>
    public Guid Id { get; init; }
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
public record CartDetailResponse : CartResponseBase
{
    /// <summary>Line items currently in the cart.</summary>
    public List<CartItem> Items { get; init; } = [];
}

/// <summary>Cart list item — lightweight summary for cart list views without line items.</summary>
public record CartListItemResponse : CartResponseBase;

/// <summary>Applied shipping adjustment summary — the server-calculated shipping cost applied to the cart/order.</summary>
public sealed record ShippingAdjustmentSummary
{
    public Guid Id { get; init; }
    public string Label { get; init; } = string.Empty;
    public decimal Amount { get; init; }
    public Guid? ShippingMethodId { get; init; }
}

/// <summary>Shipping calculation metadata captured when the shipping cost was last applied.</summary>
public sealed record ShippingCalculationSummary
{
    public decimal TotalWeight { get; init; }
    public Guid? ShippingRateId { get; init; }
    public decimal Cost { get; init; }
    public bool IsFreeShipping { get; init; }
}

/// <summary>Persisted adjustment row exposed on cart/order responses (e.g. shipping cost, future discounts).</summary>
public sealed record AdjustmentSummary
{
    public Guid Id { get; init; }
    public string Label { get; init; } = string.Empty;
    public decimal Amount { get; init; }
    public string SourceType { get; init; } = string.Empty;
    public Guid? ShippingMethodId { get; init; }
}