using Module.Ordering.Domain.Orders;

namespace Module.Ordering.Features.Storefront.Cart.Shared.Models;

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
    /// <summary>Current checkout step (e.g. address, delivery, payment, confirm, complete).</summary>
    public string CheckoutState { get; init; } = string.Empty;
    /// <summary>Selected shipping method id, if any.</summary>
    public Guid? ShippingMethodId { get; init; }
    /// <summary>Shipping address id, if any.</summary>
    public Guid? ShipAddressId { get; init; }
    /// <summary>Checkout email, if any.</summary>
    public string? Email { get; init; }
}