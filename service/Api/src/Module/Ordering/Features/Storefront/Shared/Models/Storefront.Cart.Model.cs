using Module.Ordering.Domain.Orders;

namespace Module.Ordering.Features.Storefront.Shared.Models;

/// <summary>Input parameters for cart operations — identifies the variant, quantity, and optional notes.</summary>
public abstract record CartParameters
{
    /// <summary>The product variant to add to the cart.</summary>
    public Guid VariantId { get; init; }
    /// <summary>Number of units to add; defaults to 1.</summary>
    public int Quantity { get; init; } = 1;
    /// <summary>Optional notes attached to the cart item (e.g. gift wrap request).</summary>
    public string? Notes { get; init; }
}

/// <summary>Cart request DTO — inherits cart parameters (variant ID, quantity, notes) for input validation.</summary>
public record CartRequest : CartParameters;

/// <summary>Base response model for cart operations — carries computed totals and checkout state.</summary>
public abstract record CartResponseBase
{
    /// <summary>Cart identifier.</summary>
    public Guid Id { get; init; }
    /// <summary>Sum of all line item totals before adjustments.</summary>
    public decimal ItemTotal { get; init; }
    /// <summary>Grand total after adjustments and shipping.</summary>
    public decimal Total { get; init; }
    /// <summary>ISO 4217 currency code for all monetary values.</summary>
    public string Currency { get; init; } = OrderConstant.Defaults.Currency;
    /// <summary>Total number of line items in the cart.</summary>
    public int ItemCount { get; init; }
    /// <summary>Current checkout step (Address, PickDeliveryMethod, PickPaymentMethod, Confirm, Complete).</summary>
    public CheckoutState CheckoutState { get; init; }
    /// <summary>Selected shipping method id, if any.</summary>
    public Guid? ShippingMethodId { get; init; }
    /// <summary>Shipping address id, if any.</summary>
    public Guid? ShipAddressId { get; init; }
    /// <summary>Checkout email, if any.</summary>
    public string? Email { get; init; }
    /// <summary>Applied shipping cost (sum of eligible shipping adjustments).</summary>
    public decimal ShipmentTotal { get; init; }
    /// <summary>Non-shipping adjustment total.</summary>
    public decimal AdjustmentTotal { get; init; }
    /// <summary>Applied shipping adjustment metadata, if any.</summary>
    public ShippingAdjustmentSummary? ShippingAdjustment { get; init; }
    /// <summary>Shipping calculation metadata (weight, applied rate, free state), if shipping was applied.</summary>
    public ShippingCalculationSummary? ShippingCalculation { get; init; }
    /// <summary>Persisted adjustment rows (e.g. the applied shipping cost, future discounts).</summary>
    public List<AdjustmentSummary> Adjustments { get; init; } = [];
}

/// <summary>Line item within a cart response — identifies the variant, quantity, and computed totals.</summary>
public record CartItem
{
    /// <summary>The product variant added to the cart.</summary>
    public Guid Id { get; init; }
    /// <summary>The product variant added to the cart.</summary>
    public Guid VariantId { get; init; }
    /// <summary>Parent product id, for linking to the product detail page.</summary>
    public Guid? ProductId { get; init; }
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
