using Module.Ordering.Domain.Orders;

namespace Module.Ordering.Features.Storefront.Cart.Shared.Models;

/// <summary>Represents a single item in the cart with variant and pricing details.</summary>
public class CartItem
{
    /// <summary>Line item identifier.</summary>
    public Guid Id { get; init; }
    /// <summary>Variant (SKU) identifier.</summary>
    public Guid VariantId { get; init; }
    /// <summary>Display name of the variant.</summary>
    public string VariantName { get; init; } = string.Empty;
    /// <summary>Stock-keeping unit code.</summary>
    public string Sku { get; init; } = string.Empty;
    /// <summary>Quantity selected for this item.</summary>
    public int Quantity { get; init; }
    /// <summary>Unit price of the variant.</summary>
    public decimal Price { get; init; }
    /// <summary>Line total (Price × Quantity).</summary>
    public decimal Total { get; init; }
}

/// <summary>Cart detail response with items, totals, and checkout state.</summary>
public class CartDetailResponse : CartParameters
{
    /// <summary>Cart (order) identifier.</summary>
    public Guid Id { get; init; }
    /// <summary>Line items in the cart.</summary>
    public List<CartItem> Items { get; init; } = [];
    /// <summary>Sum of all line item totals before adjustments.</summary>
    public decimal ItemTotal { get; init; }
    /// <summary>Cart grand total including adjustments.</summary>
    public decimal Total { get; init; }
    /// <summary>Currency code (default: USD).</summary>
    public string Currency { get; init; } = OrderConstant.Defaults.Currency;
    /// <summary>Number of distinct line items.</summary>
    public int ItemCount { get; init; }
    /// <summary>Checkout readiness state (Draft, Ready, Invalid).</summary>
    public string CheckoutState { get; init; } = string.Empty;
}

/// <summary>Lightweight cart list item for order history views.</summary>
public class CartListItemResponse : CartParameters
{
    /// <summary>Cart (order) identifier.</summary>
    public Guid Id { get; init; }
}
