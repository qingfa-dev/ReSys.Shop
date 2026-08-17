using Module.Ordering.Domain.Orders;
using Module.Ordering.Features.Storefront.Shared.Models;
using Shared.Application.Domain.Orders;
using Module.Billing.Domain.PaymentCaptures;
using Module.Shipping.Domain.Shipments;

namespace Module.Ordering.Features.Admin.Shared.Models;

/// <summary>Parameters for cancelling an order.</summary>
public abstract record OrderCancellationParameters
{
    /// <summary>Optional free-text reason for the cancellation — recorded for audit trail.</summary>
    public string? Reason { get; init; }
}

/// <summary>Parameters for updating an order's status.</summary>
public abstract record OrderStatusUpdateParameters
{
    /// <summary>The new order status.</summary>
    public OrderStatus Status { get; init; }
}

/// <summary>Parameters for associating a guest order with a signed-in user.</summary>
public abstract record CartAssociationParameters
{
    /// <summary>The guest order to associate.</summary>
    public Guid GuestOrderId { get; init; }
}

/// <summary>Parameters for creating an order from the cart.</summary>
public abstract record OrderCreationParameters
{
    /// <summary>The payment intent that settled the order.</summary>
    public string? PaymentIntentId { get; init; }
}

/// <summary>Parameters for actions that target an order address by identifier.</summary>
public abstract record OrderAddressActionParameters
{
    /// <summary>The address to act on.</summary>
    public Guid AddressId { get; init; }
}

public abstract record OrderParameters
{
    public string Currency { get; init; } = OrderConstant.Defaults.Currency;
    public string? Email { get; init; }
    public string? SpecialInstructions { get; init; }
    public Guid? BillAddressId { get; init; }
    public Guid? ShipAddressId { get; init; }
    public Guid? ShippingMethodId { get; init; }
}

/// <summary>Parameters for actions that set a line item quantity.</summary>
public abstract record LineItemQuantityParameters
{
    /// <summary>The new quantity for the line item.</summary>
    public int Quantity { get; init; }
}

public record OrderRequest : OrderParameters;

public record OrderDetailResponse : OrderParameters
{
    public Guid Id { get; init; }
    public string Number { get; init; } = string.Empty;
    public OrderStatus Status { get; init; }
    public CheckoutState CheckoutState { get; init; }
    public decimal ItemTotal { get; init; }
    public decimal AdjustmentTotal { get; init; }
    public decimal ShipmentTotal { get; init; }
    /// <summary>Applied shipping adjustment metadata, if any.</summary>
    public ShippingAdjustmentSummary? ShippingAdjustment { get; init; }
    /// <summary>Shipping calculation metadata (weight, applied rate, free state), if shipping was applied.</summary>
    public ShippingCalculationSummary? ShippingCalculation { get; init; }
    /// <summary>Persisted adjustment rows (e.g. the applied shipping cost, future discounts).</summary>
    public List<AdjustmentSummary> Adjustments { get; init; } = [];
    public decimal Total { get; init; }
    public decimal PaymentTotal { get; init; }
    public decimal OutstandingBalance { get; init; }
    public OrderPaymentState? PaymentState { get; init; }
    public ShipmentState? FulfillmentState { get; init; }
    public Guid? UserId { get; init; }
    public int ItemCount { get; init; }
    public Guid? ApprovedById { get; init; }
    public DateTimeOffset? ApprovedAtUtc { get; init; }
    public DateTimeOffset? CompletedAtUtc { get; init; }
    public DateTimeOffset? CanceledAtUtc { get; init; }
    public DateTimeOffset CreatedAtUtc { get; init; }
    public DateTimeOffset? ModifiedAtUtc { get; init; }
    public DateTimeOffset? PaymentProcessingAtUtc { get; init; }
    public DateTimeOffset? PaymentCompletedAtUtc { get; init; }
    public DateTimeOffset? PaymentFailedAtUtc { get; init; }
    public DateTimeOffset? ShipmentShippedAtUtc { get; init; }
    public DateTimeOffset? ShipmentDeliveredAtUtc { get; init; }
    public List<PaymentCaptureSummary> Payments { get; init; } = [];
    public List<ShipmentSummary> Shipments { get; init; } = [];
    public List<OrderTimelineEvent> Timeline { get; init; } = [];
    public List<LineItemResponse> LineItems { get; init; } = [];
}

public record OrderListItemResponse : OrderParameters
{
    public Guid Id { get; init; }
    public string Number { get; init; } = string.Empty;
    public OrderStatus Status { get; init; }
    public decimal Total { get; init; }
    public decimal PaymentTotal { get; init; }
    public OrderPaymentState? PaymentState { get; init; }
    public ShipmentState? FulfillmentState { get; init; }
    public DateTimeOffset CreatedAtUtc { get; init; }
    public DateTimeOffset? CompletedAtUtc { get; init; }
}

public record LineItemResponse
{
    public Guid Id { get; init; }
    public int Quantity { get; set; }
    public decimal Price { get; set; }
    public decimal Total { get; set; }
    public decimal AdjustmentTotal { get; set; }
    public string Currency { get; set; } = OrderConstant.Defaults.Currency;

    public Guid OrderId { get; set; }
    public Guid? VariantId { get; set; }
    public Guid? ProductId { get; set; }
    public string? ProductName { get; set; }
    public string? ProductImageUrl { get; set; }
    public DateTimeOffset CreatedAtUtc { get; init; }
}

public sealed record PaymentCaptureSummary
{
    public Guid Id { get; init; }
    public string Number { get; init; } = string.Empty;
    public decimal Amount { get; init; }
    public string Currency { get; init; } = string.Empty;
    public PaymentRecordState State { get; init; }
    public string? PaymentStatus { get; init; }
    public string ProviderKey { get; init; } = string.Empty;
    public Guid? PaymentMethodId { get; init; }
    public DateTimeOffset CreatedAtUtc { get; init; }
    public DateTimeOffset? CompletedAtUtc { get; init; }
    public DateTimeOffset? FailedAtUtc { get; init; }
}

public sealed record ShipmentSummary
{
    public Guid Id { get; init; }
    public Guid OrderId { get; init; }
    public Guid ShippingMethodId { get; init; }
    public string? ShippingMethodName { get; init; }
    public string TrackingNumber { get; init; } = string.Empty;
    public ShipmentStatus Status { get; init; }
    public DateTimeOffset? ShippedAtUtc { get; init; }
    public DateTimeOffset? DeliveredAtUtc { get; init; }
    public DateTimeOffset? EstimatedDeliveryAtUtc { get; init; }
    public DateTimeOffset CreatedAtUtc { get; init; }
}

public sealed record OrderTimelineEvent
{
    public string Type { get; init; } = string.Empty;
    public string Label { get; init; } = string.Empty;
    public DateTimeOffset? OccurredAtUtc { get; init; }
}

/// <summary>Parameters for actions that select a shipping method by identifier.</summary>
public abstract record ShippingMethodActionParameters
{
    /// <summary>The shipping method to select.</summary>
    public Guid ShippingMethodId { get; init; }
}
